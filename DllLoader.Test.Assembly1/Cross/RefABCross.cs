using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefABCross", "Rube200", "1.0.0")]
    [Description("RefABCross is for testing")]
    public class RefABCross : RustPlugin
    {
        [PluginReference]
        Plugin RefAACross;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAACross != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAACross != null}");
            RefAACross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}