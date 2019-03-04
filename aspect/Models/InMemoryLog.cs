using System.Collections.ObjectModel;

using Serilog.Core;
using Serilog.Events;

namespace Aspect.Models
{
    public sealed class InMemoryLog : ILogEventSink
    {
        private InMemoryLog()
        {
            mEvents = new ObservableCollection<LogEvent>();
            Events = new ReadOnlyObservableCollection<LogEvent>(mEvents);
        }

        private readonly ObservableCollection<LogEvent> mEvents;
        public ReadOnlyObservableCollection<LogEvent> Events { get; }

        public static InMemoryLog Instance { get; } = new InMemoryLog();
        public LoggingLevelSwitch LevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Warning);

        public void Emit(LogEvent logEvent) => mEvents.Add(logEvent);

        public void Clear() => mEvents.Clear();
    }
}
