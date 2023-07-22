#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefACCircularCross", "Rube200", "1.0.0")]
    [Description("RefACCircularCross is for testing")]
    public class RefACCircularCross : RustPlugin
    {
        [PluginReference] private Plugin RefADCircularCross;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefADCircularCross != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefADCircularCross != null}");
                RefADCircularCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}