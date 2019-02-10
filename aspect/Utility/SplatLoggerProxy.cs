using Serilog;
using Serilog.Events;

using Splat;

using ILogger = Splat.ILogger;

namespace Aspect.Utility
{
    public sealed class SplatLoggerProxy : ILogger
    {
        public LogLevel Level { get; set; }

        public void Write(string message, LogLevel logLevel) =>
            Log.Write(_Translate(logLevel), "{Message}", message);

        private LogEventLevel _Translate(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug: return LogEventLevel.Debug;
                case LogLevel.Info: return LogEventLevel.Information;
                case LogLevel.Warn: return LogEventLevel.Warning;
                case LogLevel.Error: return LogEventLevel.Error;
                case LogLevel.Fatal: return LogEventLevel.Fatal;
                default: return LogEventLevel.Verbose;
            }
        }
    }
}
