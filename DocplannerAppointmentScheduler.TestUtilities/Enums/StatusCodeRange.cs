namespace DocplannerAppointmentScheduler.TestUtilities.Enums
{
    [Flags]
    public enum StatusCodeRange
    {
        None = 0,
        Informational = 1, // 1xx
        Success = 2,       // 2xx
        Redirection = 4,   // 3xx
        ClientError = 8,   // 4xx
        ServerError = 16,  // 5xx
        All = Informational | Success | Redirection | ClientError | ServerError, // All ranges combined
        AllButSuccess = Informational | Redirection | ClientError | ServerError // All except Success
    }
}
