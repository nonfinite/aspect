using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

using Aspect.Models;
using Aspect.Utility;

using Serilog.Events;

namespace Aspect.UI
{
    public sealed class LogViewModel : NotifyPropertyChanged
    {
        public LogViewModel()
        {
        }

        public ReadOnlyObservableCollection<LogEvent> Events => InMemoryLog.Instance.Events;
    }

    public sealed class LogEventView : StackPanel
    {
        public LogEventView(LogEvent evt)
        {
            
        }
    }
}
