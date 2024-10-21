using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.HttpLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicAPI.Attributes
{

    /// <summary>
    /// Indicates DynamicAPI to skip this method for server controller.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreInDynamicAPIAttribute : System.Attribute
    {
       
        /// <summary>
        /// Use authorization for interface / class and / or method.
        /// </summary>
        /// <param name="policies">Name(s) of the policies to hit allowing to call the endpoint.</param>
        public IgnoreInDynamicAPIAttribute()
        {
        }
    }

    /// <summary>
    /// Indicates DynamicAPI to require authorization to call this endpoint.
    /// The attribute can be set to the interface / class and / or to a method.
    /// If the attribute is set to the interface / class and the method, calling the method will require to hit all the policies even if multiple policies are set in one attribute.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAuthorizationAttribute : System.Attribute
    {
        public string[] Policies { get; private set; }

        /// <summary>
        /// Use authorization for interface / class and / or method.
        /// </summary>
        /// <param name="policies">Name(s) of the policies to hit allowing to call the endpoint.</param>
        public RequireAuthorizationAttribute(params string[] policies)
        {
            Policies = policies;
        }
    }

    /// <summary>
    /// Set information for this endpoint.
    /// The attribute can be set to the interface / class and / or a method.
    /// If it's set to interface / class and method, the settings for the method will override the upper settings just for this method.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class InformationAttribute : System.Attribute
    {
        //public string? Name { get; private set; }
        //public string? DisplayName { get; private set; }
        public string? GroupName { get; private set; }
        public string? Description { get; private set; }
        public string? Summary { get; private set; }
        public int Order { get; private set; }

        /// <summary>
        /// Set informational properties for this endpoint on interface / class and / or method.
        /// </summary>
        /// <param name="groupName">Name for grouping the endpoint. <see href="https://github.com/dotnet/aspnetcore/issues/36414">Look at GitHub for further information</see></param>
        /// <param name="description">Description</param>
        /// <param name="summary">Summary</param>
        /// <param name="order">Order</param>
        public InformationAttribute(string? groupName = null, string? description = null, string? summary = null, int order = -1)
        {
            //Name = name;
            //DisplayName = displayName;
            GroupName = groupName;
            Description = description;
            Summary = summary;
            Order = order;
        }

    }


    /// <summary>
    /// Enables logging for this endpoint.
    /// The attribute can be set to the interface / class and / or a method.
    /// If it's set to interface / class and method, the settings for the method will override the upper settings just for this method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class HttpLoggingAttribute : System.Attribute
    {
        public HttpLoggingFields LoggingFields { get; private set; }
        public int? RequestBodyLimit { get; private set; }
        public int? ResponseBodyLimit { get; private set; }

        /// <summary>
        /// Enable logging for this endpoint
        /// </summary>
        /// <param name="loggingFields"></param>
        /// <param name="requestBodyLimit">Set the request body limit for the endpoint. If ist set to -1 results in using the default value defined for global HttpLogging.</param>
        /// <param name="responseBodyLimit">Set the request body limit for the endpoint. If ist set to -1 results in using the default value defined for global HttpLogging.</param>
        public HttpLoggingAttribute(HttpLoggingFields loggingFields, int? requestBodyLimit = null, int? responseBodyLimit = null)
        {
            LoggingFields = loggingFields;
            RequestBodyLimit = requestBodyLimit;
            ResponseBodyLimit = responseBodyLimit;
        }
    }


    /// <summary>
    /// Request timeout for this endpoint.
    /// The attribute can be set to the interface / class and / or a method.
    /// If it's set to interface / class and method, the settings for the method will override the upper settings just for this method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequestTimeoutAttribute : System.Attribute
    {
        public RequestTimeoutPolicy? Policy { get; private set; }
        public string? PolicyName { get; private set; }
        public TimeSpan? TimeSpan { get; private set; }

        public bool Disable { get; private set; }

        /// <summary>
        /// Disables request timeout on the endpoint(s).
        /// </summary>
        /// <param name="disable">True if the timeout should be disabled</param>
        public RequestTimeoutAttribute(bool disable)
        {
            Disable = disable;
        }

        /// <summary>
        /// Set request time out
        /// </summary>
        /// <param name="policy">Policy for timeout</param>
        public RequestTimeoutAttribute(RequestTimeoutPolicy policy)
        {
            Policy = policy;
        }

        /// <summary>
        /// Set request time out
        /// </summary>
        /// <param name="policyName">Name of the time out policy of defined globaly</param>
        public RequestTimeoutAttribute(string policyName)
        {
            PolicyName = policyName;
        }

        /// <summary>
        /// Set request time out
        /// </summary>
        /// <param name="timeSpan">Time out after time span</param>
        public RequestTimeoutAttribute(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }
    }

    /// <summary>
    /// Set tags for the endpoint on interface / class and / or method.
    /// If tags set for class and method, the tags will be merged.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TagsAttribute : System.Attribute
    {
        public string[] Tags { get; private set; }

        /// <summary>
        /// Set tags for endpoint
        /// </summary>
        /// <param name="policies">Tags to set</param>
        public TagsAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }


    /// <summary>
    /// Set meta data for the endpoint on interface / class and / or method.
    /// If meta data set for class and method, the tags will be merged.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Interface | System.AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class MetaDataAttribute : System.Attribute
    {
        public object[] Items { get; private set; }

        /// <summary>
        /// Set meta data for endpoint
        /// </summary>
        /// <param name="policies">Items to set</param>
        public MetaDataAttribute(params object[] items)
        {
            Items = items;
        }
    }

}
