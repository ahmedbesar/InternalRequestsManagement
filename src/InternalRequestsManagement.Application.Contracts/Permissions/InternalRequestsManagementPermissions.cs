namespace InternalRequestsManagement.Permissions;

public static class InternalRequestsManagementPermissions
{
    public const string GroupName = "InternalRequestsManagement";

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
    }

    public static class Requests
    {
        public const string Default = GroupName + ".Requests";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ChangeStatus = Default + ".ChangeStatus";
        public const string Assign = Default + ".Assign";
    }

}
