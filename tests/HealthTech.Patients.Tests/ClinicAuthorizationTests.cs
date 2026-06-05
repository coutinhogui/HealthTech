using HealthTech.BuildingBlocks.Abstractions;
using Xunit;

namespace HealthTech.Patients.Tests;

public sealed class ClinicAuthorizationTests
{
    [Theory]
    [InlineData("admin", ClinicPermission.ManageAccess, true)]
    [InlineData("admin", ClinicPermission.ManageBilling, true)]
    [InlineData("professional", ClinicPermission.ReadOwnClinicalSchedule, true)]
    [InlineData("professional", ClinicPermission.ManageBilling, false)]
    [InlineData("reception", ClinicPermission.ManageSchedule, true)]
    [InlineData("reception", ClinicPermission.ManageBilling, false)]
    [InlineData("billing", ClinicPermission.ManageBilling, true)]
    [InlineData("billing", ClinicPermission.ReadClinicalRecord, false)]
    [InlineData("patient", ClinicPermission.ReadOwnPatientProfile, true)]
    [InlineData("patient", ClinicPermission.ReadPatients, false)]
    [InlineData("patient", ClinicPermission.ManageSchedule, false)]
    public void Role_matrix_maps_roles_to_expected_permissions(
        string role,
        ClinicPermission permission,
        bool expected)
    {
        Assert.Equal(expected, ClinicAuthorization.RoleHasPermission(role, permission));
    }

    [Theory]
    [InlineData("system_admin", GlobalPermission.ManageClinics, true)]
    [InlineData("system_admin", GlobalPermission.ManageSystemAdmins, true)]
    [InlineData("admin", GlobalPermission.ManageClinics, false)]
    [InlineData("patient", GlobalPermission.ManageClinics, false)]
    public void Global_role_matrix_maps_system_admin_permissions(
        string role,
        GlobalPermission permission,
        bool expected)
    {
        Assert.Equal(expected, ClinicAuthorization.GlobalRoleHasPermission(role, permission));
    }
}
