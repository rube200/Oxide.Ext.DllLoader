using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefSDeep", "Rube200", "1.0.0")]
    [Description("RefSDeep is for testing")]
    public class RefSDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefTDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefTDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefTDeep != null}");
            RefTDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}