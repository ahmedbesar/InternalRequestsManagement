namespace InternalRequestsManagement;

public static class InternalRequestsManagementDomainErrorCodes
{
    public const string OrganizationUnitRequired = "InternalRequestsManagement:OrganizationUnitRequired";
    public const string InvalidStatusTransition = "InternalRequestsManagement:InvalidStatusTransition";
    public const string JustificationRequired = "InternalRequestsManagement:JustificationRequired";
    public const string DueDateRequired = "InternalRequestsManagement:DueDateRequired";
    public const string StatusNoteRequired = "InternalRequestsManagement:StatusNoteRequired";
    public const string AssigneeNotInOrganizationUnit = "InternalRequestsManagement:AssigneeNotInOrganizationUnit";
    public const string RequestTypeNotAvailableForOrganizationUnit = "InternalRequestsManagement:RequestTypeNotAvailableForOrganizationUnit";
    public const string RequestAlreadyInTerminalStatus = "InternalRequestsManagement:RequestAlreadyInTerminalStatus";
    public const string RequestNotFound = "InternalRequestsManagement:RequestNotFound";
    public const string RequestTypeNotFound = "InternalRequestsManagement:RequestTypeNotFound";
}
