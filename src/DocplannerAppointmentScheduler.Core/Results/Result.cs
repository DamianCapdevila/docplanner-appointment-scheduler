namespace DocplannerAppointmentScheduler.Core.Results
{
    public class Result<T>
    {
        public bool IsSuccess => Error == null;
        public T? Value { get; }
        public Error? Error { get; }

        private Result(T value)
        {
            Error = null;
            Value = value;
        }

        private Result(Error error)
        {
            Error = error;
            Value = default;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);

        public TResult Match<TResult>(Func<T, TResult> success, Func<Error, TResult> failure)
        {
            return IsSuccess ? success(Value!) : failure(Error!);
        }
    }
}
