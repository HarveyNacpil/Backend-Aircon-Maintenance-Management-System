using Microsoft.AspNetCore.Authorization;

namespace Api.AuthorizationModule
{
    /// <summary>
    /// Centralized policy name constants and policy registration for the aircon schema.
    /// Roles are stored as TEXT: Admin | Technician | Staff
    /// </summary>
    public static class AppPolicies
    {
        public const string AdminOnly       = "AdminOnly";
        public const string TechnicianOnly  = "TechnicianOnly";
        public const string StaffOnly       = "StaffOnly";
        public const string AdminOrTech     = "AdminOrTech";
        public const string AllRoles        = "AllRoles";

        public static void AddAppPolicies(this AuthorizationOptions options)
        {
            // Require "user_role" claim matching the role string from JWT
            options.AddPolicy(AdminOnly,      p => p.AddRequirements(new RoleRequirement("Admin")));
            options.AddPolicy(TechnicianOnly, p => p.AddRequirements(new RoleRequirement("Technician")));
            options.AddPolicy(StaffOnly,      p => p.AddRequirements(new RoleRequirement("Staff")));
            options.AddPolicy(AdminOrTech,    p => p.AddRequirements(new RoleRequirement("Admin", "Technician")));
            options.AddPolicy(AllRoles,       p => p.AddRequirements(new RoleRequirement("Admin", "Technician", "Staff")));
        }
    }
}
