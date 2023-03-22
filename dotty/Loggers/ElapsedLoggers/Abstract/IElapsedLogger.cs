namespace dotty.Loggers.ElapsedLoggers.Abstract
{
    public interface IElapsedLogger
    {
        public void Start(string id, string? message = null);
        public void Stop(string id, string? message = null);
    }
}
