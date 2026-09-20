namespace HealthTech.BuildingBlocks.Abstractions;

public static class ClinicRoles
{
    public const string Admin = "admin";
    public const string Professional = "professional";
    public const string Reception = "reception";
    public const string Billing = "billing";
    public const string Patient = "patient";
    public const string SystemAdmin = "system_admin";
}

public enum GlobalPermission
{
    ManageClinics,
    ManageSystemAdmins
}

public enum ClinicPermission
{
    ManageAccess,
    ManagePatients,
    ReadPatients,
    ReadBillingContext,
    ManageSchedule,
    ReadOwnClinicalSchedule,
    ReadClinicalRecord,
    WriteClinicalRecord,
    ManageBilling,
    ManageProfessionals,
    ReadOwnPatientProfile,
    ManageOwnAppointments
}

public static class ClinicAuthorization
{
    public static bool GlobalRoleHasPermission(string? role, GlobalPermission permission)
        => NormalizeRole(role) == ClinicRoles.SystemAdmin &&
           permission is GlobalPermission.ManageClinics or GlobalPermission.ManageSystemAdmins;

    public static IReadOnlyCollection<string> GlobalPermissionNamesForRole(string? role)
        => Enum.GetValues<GlobalPermission>()
            .Where(permission => GlobalRoleHasPermission(role, permission))
            .Select(static permission => permission.ToString())
            .ToArray();

    public static bool RoleHasPermission(string? role, ClinicPermission permission)
        => NormalizeRole(role) switch
        {
            ClinicRoles.Admin => true,
            ClinicRoles.Professional => permission is
                ClinicPermission.ReadPatients or
                ClinicPermission.ReadOwnClinicalSchedule or
                ClinicPermission.ReadClinicalRecord or
                ClinicPermission.WriteClinicalRecord,
            ClinicRoles.Reception => permission is
                ClinicPermission.ManagePatients or
                ClinicPermission.ReadPatients or
                ClinicPermission.ManageSchedule or
                ClinicPermission.ReadBillingContext,
            ClinicRoles.Billing => permission is
                ClinicPermission.ManageBilling or
                ClinicPermission.ReadBillingContext or
                ClinicPermission.ReadPatients,
            ClinicRoles.Patient => permission is
                ClinicPermission.ReadOwnPatientProfile or
                ClinicPermission.ManageOwnAppointments,
            _ => false
        };

    public static IReadOnlyCollection<string> PermissionNamesForRole(string? role)
        => Enum.GetValues<ClinicPermission>()
            .Where(permission => RoleHasPermission(role, permission))
            .Select(static permission => permission.ToString())
            .ToArray();

    public static bool HasPermission(this ResolvedRequestContext context, ClinicPermission permission)
        => context.ActiveMembership is not null && RoleHasPermission(context.ActiveMembership.Role, permission);

    public static bool IsProfessionalScoped(this ResolvedRequestContext context)
        => string.Equals(NormalizeRole(context.ActiveMembership?.Role), ClinicRoles.Professional, StringComparison.Ordinal);

    public static bool IsPatientScoped(this ResolvedRequestContext context)
        => string.Equals(NormalizeRole(context.ActiveMembership?.Role), ClinicRoles.Patient, StringComparison.Ordinal);

    public static string NormalizeRole(string? role)
        => string.IsNullOrWhiteSpace(role) ? string.Empty : role.Trim().ToLowerInvariant();
}
