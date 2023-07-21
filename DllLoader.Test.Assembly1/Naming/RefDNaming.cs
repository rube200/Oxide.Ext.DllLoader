using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Test.Naming
{
    [Info("RefDNaming", "Rube200", "1.0.0")]
    [Description("RefDNaming is for testing")]
    public class RefDNaming : RustPlugin
    {
        [PluginReference("RefENaming")]
        Plugin plugin;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {plugin != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {plugin != null}");
            plugin?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}