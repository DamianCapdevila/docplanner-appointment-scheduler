namespace DocplannerAppointmentScheduler.Core.Exceptions
{
    public class MissingEnvironmentVariableException : Exception
    {
        public MissingEnvironmentVariableException(string message) : base(message) { }
    }
}
