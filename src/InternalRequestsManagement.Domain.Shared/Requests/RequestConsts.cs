namespace InternalRequestsManagement.Requests;

public static class RequestConsts
{
    public const int MinTitleLength = 3;
    public const int MaxTitleLength = 256;
    public const int MaxDescriptionLength = 4000;
    public const int MaxJustificationLength = 2000;
    public const int MaxStatusNoteLength = 2000;

    public static readonly RequestStatus[] TerminalStatuses =
    [
        RequestStatus.Closed,
        RequestStatus.Cancelled,
        RequestStatus.Rejected
    ];

    public static readonly RequestStatus[] NoteRequiredStatuses =
    [
        RequestStatus.OnHold,
        RequestStatus.Rejected,
        RequestStatus.Cancelled
    ];

    public static bool IsTerminal(RequestStatus status) =>
        System.Array.IndexOf(TerminalStatuses, status) >= 0;

    public static bool RequiresNote(RequestStatus toStatus) =>
        System.Array.IndexOf(NoteRequiredStatuses, toStatus) >= 0;
}
