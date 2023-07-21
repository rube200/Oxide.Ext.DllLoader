using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefADCircularCross", "Rube200", "1.0.0")]
    [Description("RefADCircularCross is for testing")]
    public class RefADCircularCross : RustPlugin
    {
        [PluginReference]
        Plugin RefACCircularCross;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefACCircularCross != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefACCircularCross != null}");
            RefACCircularCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}