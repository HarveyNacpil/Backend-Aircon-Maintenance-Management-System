using Microsoft.AspNetCore.Authorization;

namespace Api.AuthorizationModule
{
    /// <summary>
    /// Requirement that the "user_role" JWT claim matches at least one of the allowed role names.
    /// </summary>
    public sealed class RoleRequirement : IAuthorizationRequirement
    {
        public IReadOnlyList<string> AllowedRoles { get; }

        public RoleRequirement(params string[] allowedRoles)
        {
            if (allowedRoles == null || allowedRoles.Length == 0)
                throw new ArgumentException("At least one role must be specified.", nameof(allowedRoles));
            AllowedRoles = allowedRoles;
        }
    }

    /// <summary>
    /// Handles <see cref="RoleRequirement"/> by checking the "user_role" claim in the JWT.
    /// </summary>
    public sealed class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleRequirement requirement)
        {
            var roleClaim = context.User.FindFirst("user_role")?.Value;

            if (!string.IsNullOrEmpty(roleClaim) &&
                requirement.AllowedRoles.Contains(roleClaim, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
