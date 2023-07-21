using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefTDeep", "Rube200", "1.0.0")]
    [Description("RefTDeep is for testing")]
    public class RefTDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefUDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefUDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefUDeep != null}");
            RefUDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}