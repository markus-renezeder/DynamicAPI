using Microsoft.AspNetCore.Authorization;
using Server.Models;

namespace Server.Services
{
    public class AuthorizationHandler : AuthorizationHandler<AuthorizationRequirement>
    {

        public AuthorizationHandler()
        {
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement)
        {
            try
            {
                if(requirement.Permission == "user")
                {
                    context.Succeed(requirement);
                }
                else
                {
                    AuthorizationFailureReason reason = new AuthorizationFailureReason(this, "Policy 'admin' required");

                    context.Fail(reason);
                }
                

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


    }
}
