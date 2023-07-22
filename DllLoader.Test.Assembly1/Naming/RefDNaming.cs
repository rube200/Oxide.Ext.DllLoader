#region

using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Test.Naming
{
    [Info("RefDNaming", "Rube200", "1.0.0")]
    [Description("RefDNaming is for testing")]
    public class RefDNaming : RustPlugin
    {
        [PluginReference("RefENaming")] private Plugin plugin;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {plugin != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {plugin != null}");
                plugin?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}