using dotty.Exceptions;
using dotty.Loggers.ElapsedLoggers.Abstract;
using System.Diagnostics;

namespace dotty.Loggers.ElapsedLoggers
{
    public class ElapsedLogger : IElapsedLogger
    {
        private readonly ILogger<ElapsedLogger> _logger;
        private readonly Dictionary<string, Stopwatch> _idToStopwatch;

        public ElapsedLogger(ILogger<ElapsedLogger> logger)
        {
            _logger = logger;
            this._idToStopwatch = new Dictionary<string, Stopwatch>();
        }

        public void Start(string id, string? message = null)
        {
            if (this._idToStopwatch.ContainsKey(id))
            {
                throw new ElapsedLoggerException($"Start was called with an ID that already exists: {id}");
            }

            var stopwatch = new Stopwatch();

            this._idToStopwatch.Add(id, stopwatch);
            this._logger.LogInformation(message);

            stopwatch.Start();
        }

        public void Stop(string id, string? message = null)
        {
            Stopwatch stopwatch;

            if (!this._idToStopwatch.TryGetValue(id, out stopwatch))
            {
                throw new ElapsedLoggerException($"Stop was called for an ID that does not exist: {id}");
            }

            stopwatch.Stop();
            
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds.ToString();

            this._idToStopwatch.Remove(id);

            this._logger.LogInformation($"{(message == null ? id : message)} Elapsed: {elapsedMilliseconds}");
        }
    }
}
