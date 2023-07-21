using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefCOrder", "Rube200", "1.0.0")]
    [Description("RefCOrder is for testing")]
    public class RefCOrder : RustPlugin
    {
        [PluginReference]
        Plugin RefBOrder;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefBOrder != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefBOrder != null}");
            RefBOrder?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}