namespace dotty.Exceptions
{
    public class ElapsedLoggerException : Exception
    {
        public ElapsedLoggerException() { }
        public ElapsedLoggerException(string message)
            : base(message)
        { }
        public ElapsedLoggerException(string message, Exception inner)
        : base(message, inner)
        { }
    }
}
