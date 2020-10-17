#region

using System.IO;
using Oxide.Core.Libraries;

#endregion

namespace Oxide.Ext.DllLoader
{
    public class QueuedChange
    {
        public WatcherChangeTypes ChangeType;
        public Timer.TimerInstance OxideTimer;
    }
}