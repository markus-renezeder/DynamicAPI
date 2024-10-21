using Microsoft.AspNetCore.Authorization;

namespace Server.Models
{
    public class AuthorizationRequirement : IAuthorizationRequirement
    {
        private readonly string _Permission;
        
        public string Permission
        {
            get { return _Permission; }
        }

        public AuthorizationRequirement(string permission)
        {
            _Permission = permission;
        }

    }
}
