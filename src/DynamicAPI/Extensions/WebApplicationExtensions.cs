using System.Reflection;
using System.Linq.Expressions;
using DynamicAPI.Attributes;
using DynamicAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using TagsAttribute = DynamicAPI.Attributes.TagsAttribute;
using System.Formats.Tar;
using Microsoft.AspNetCore.Routing;

namespace DynamicAPI.Extensions
{
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Create a dynamic controller for the class or interface
        /// </summary>
        /// <typeparam name="T">Interface or class</typeparam>
        /// <param name="app">Web application</param>
        /// <returns>WebApplication</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="Exception"></exception>
        public static WebApplication AddDynamicController<T>(this WebApplication app) where T : class
        {
            try
            {
                var scope = app.Services.CreateScope();

                T service = scope.ServiceProvider.GetRequiredService<T>();

                var methods = typeof(T).GetMethods();

                var interfaceAuth = typeof(T).GetCustomAttribute<RequireAuthorizationAttribute>();
                var informationAttribute = typeof(T).GetCustomAttribute<InformationAttribute>();

                if (service == null)
                {
                    throw new Exception($"Service for type '{typeof(T).FullName}' not found. Make sure that the service is injected by using 'builder.Services.Add...");
                }
                else
                {

                    var group = app.MapGroup(string.Empty);

                    if(!string.IsNullOrEmpty(informationAttribute?.Description))
                        group.WithDescription(informationAttribute.Description);

                    if (!string.IsNullOrEmpty(informationAttribute?.GroupName))
                        group.WithGroupName(informationAttribute.GroupName);

                    if (!string.IsNullOrEmpty(informationAttribute?.Summary))
                        group.WithSummary(informationAttribute.Summary);

                    if((informationAttribute?.Order ?? -1) >= 0)
                        group.WithOrder(informationAttribute?.Order ?? 0);



                    var groupHttpLoggingAttribute = typeof(T).GetCustomAttribute<HttpLoggingAttribute>();

                    if (groupHttpLoggingAttribute != null)
                        group.WithHttpLogging(groupHttpLoggingAttribute.LoggingFields, groupHttpLoggingAttribute.RequestBodyLimit, groupHttpLoggingAttribute.ResponseBodyLimit);

                    var grouptimeoutAttribute = typeof(T).GetCustomAttribute<RequestTimeoutAttribute>();

                    if (grouptimeoutAttribute != null)
                    {
                        if(grouptimeoutAttribute.Disable)
                        {
                            group.DisableRequestTimeout();
                        }
                        else if (grouptimeoutAttribute.Policy != null)
                        {
                            group.WithRequestTimeout(grouptimeoutAttribute.Policy);
                        }
                        else if (!string.IsNullOrEmpty(grouptimeoutAttribute.PolicyName))
                        {
                            group.WithRequestTimeout(grouptimeoutAttribute.PolicyName);
                        }
                        else if (grouptimeoutAttribute.TimeSpan != null)
                        {
                            group.WithRequestTimeout(grouptimeoutAttribute.TimeSpan.Value);
                        }
                    }

                    if (typeof(T).GetCustomAttribute<TagsAttribute>()?.Tags.Any() == true)
                    {
                        group.WithTags(typeof(T).GetCustomAttribute<TagsAttribute>()?.Tags);
                    }

                    if (typeof(T).GetCustomAttribute<MetaDataAttribute>()?.Items.Any() == true)
                    {
                        group.WithMetadata(typeof(T).GetCustomAttribute<MetaDataAttribute>()?.Items);
                    }


                    foreach (var method in methods.Where(m => m.GetCustomAttribute<IgnoreInDynamicAPIAttribute>() == null))
                    {
                        var methodAuth = method.GetCustomAttribute<RequireAuthorizationAttribute>();

                        var methodAttributes = method.HttpMethodAttributes();

                        foreach (var attribute in methodAttributes)
                        {
                            RouteHandlerBuilder? routeHandler = null;

                            switch (attribute.Method.Method)
                            {
                                case "GET":
                                    routeHandler = group.MapGet(attribute.Path, method.CreateDelegate(service));
                                    break;
                                case "POST":
                                    routeHandler = group.MapPost(attribute.Path, method.CreateDelegate(service));
                                    break;
                                case "PUT":
                                    routeHandler = group.MapPut(attribute.Path, method.CreateDelegate(service));
                                    break;
                                case "DELETE":
                                    routeHandler = group.MapDelete(attribute.Path, method.CreateDelegate(service));
                                    break;
                                case "PATCH":
                                    routeHandler = group.MapPatch(attribute.Path, method.CreateDelegate(service));
                                    break;
                                default:
                                    throw new NotImplementedException($"{typeof(T).FullName + """.""" + method.Name} --> Handling '{attribute.Method.Method}' is not supported by DynamicAPI at the moment. Use the 'IgnoreInDynamicAPI'-attribute to ignore this method when creating the controller.");
                            }

                            if (routeHandler == null)
                            {
                                throw new Exception($"Error creating handler for {typeof(T).FullName + """.""" + method.Name}");
                            }
                            else
                            {
                                var policies = interfaceAuth?.Policies.ToList();

                                if (methodAuth != null)
                                {
                                    var methodPolicies = methodAuth.Policies.ToList();

                                    if (policies?.Any() != true)
                                    {
                                        policies = methodPolicies;
                                    }
                                    else
                                    {
                                        policies.AddRange(methodPolicies.Where(m => !policies.Any(p => p.Equals(m, StringComparison.InvariantCultureIgnoreCase))));
                                    }
                                }

                                if (policies?.Any() == true)
                                {
                                    routeHandler.RequireAuthorization(policies.ToArray());

                                }

                                var methodInformationAttribute = method.GetCustomAttribute<InformationAttribute>();

                                if (!string.IsNullOrEmpty(methodInformationAttribute?.Description))
                                    routeHandler.WithDescription(methodInformationAttribute.Description);

                                if (!string.IsNullOrEmpty(methodInformationAttribute?.GroupName))
                                    routeHandler.WithGroupName(methodInformationAttribute.GroupName);

                                if ((methodInformationAttribute?.Order ?? -1) > -1)
                                    routeHandler.WithOrder(methodInformationAttribute?.Order ?? 0);

                                if (!string.IsNullOrEmpty(methodInformationAttribute?.Summary))
                                    routeHandler.WithSummary(methodInformationAttribute.Summary);


                                var usingHttpLoggingAttribute = method.GetCustomAttribute<HttpLoggingAttribute>();

                                if (usingHttpLoggingAttribute != null)
                                    routeHandler.WithHttpLogging(usingHttpLoggingAttribute.LoggingFields, usingHttpLoggingAttribute.RequestBodyLimit, usingHttpLoggingAttribute.ResponseBodyLimit);

                                var timeoutAttribute = method.GetCustomAttribute<RequestTimeoutAttribute>() ?? typeof(T).GetCustomAttribute<RequestTimeoutAttribute>();

                                if (method.GetCustomAttribute<RequestTimeoutAttribute>() != null)
                                {
                                    if(method.GetCustomAttribute<RequestTimeoutAttribute>()?.Disable == true)
                                    {
                                        routeHandler.DisableRequestTimeout();
                                    }
                                    else if (method.GetCustomAttribute<RequestTimeoutAttribute>()?.Policy != null)
                                    {
                                        routeHandler.WithRequestTimeout(method.GetCustomAttribute<RequestTimeoutAttribute>().Policy);
                                    }
                                    else if (!string.IsNullOrEmpty(method.GetCustomAttribute<RequestTimeoutAttribute>()?.PolicyName))
                                    {
                                        routeHandler.WithRequestTimeout(method.GetCustomAttribute<RequestTimeoutAttribute>().PolicyName);
                                    }
                                    else if (method.GetCustomAttribute<RequestTimeoutAttribute>()?.TimeSpan != null)
                                    {
                                        routeHandler.WithRequestTimeout(method.GetCustomAttribute<RequestTimeoutAttribute>().TimeSpan.Value);
                                    }
                                }

                                if (method.GetCustomAttribute<TagsAttribute>()?.Tags.Any() == true)
                                {
                                    routeHandler.WithTags(method.GetCustomAttribute<TagsAttribute>()?.Tags);
                                }

                                if (method.GetCustomAttribute<MetaDataAttribute>()?.Items.Any() == true)
                                {
                                    routeHandler.WithMetadata(method.GetCustomAttribute<MetaDataAttribute>()?.Items);
                                }

                                routeHandler.WithOpenApi();
                            }

                        }

                    }
                }

                return app;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating dynamic controller for {typeof(T).Name}. See inner exception for more details", ex);
            }
        }

        private static Delegate CreateDelegate(this MethodInfo methodInfo, object target)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        /// <summary>
        /// Enable exception handler for handling DynamicAPIException.
        /// DynamicAPIException allows to throw an exception in your service and provide a return code (e.g. 404 - not found).
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>WebApplication</returns>
        public static WebApplication UseDynamicAPIExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if(exceptionHandlerFeature != null && exceptionHandlerFeature.Error != null && exceptionHandlerFeature.Error is DynamicAPIException)
                    {
                        var exception = exceptionHandlerFeature.Error as DynamicAPIException;

                        if (exception != null)
                        {

                            await Results.Problem(detail: exception.Message, statusCode: (int)exception.StatusCode).ExecuteAsync(context);

                            return;
                        }
                        
                    }
                    
                    
                    string exceptionMessage = "Unhandled exception was thrown";

                    if(exceptionHandlerFeature != null && exceptionHandlerFeature.Error != null)
                    {
                        exceptionMessage = exceptionHandlerFeature.Error.Message;
                    }

                    await Results.Problem(detail: exceptionMessage, statusCode: 500).ExecuteAsync(context);

                }));

            return app;

        }
    }

}

    
