using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefQDeepCross", "Rube200", "1.0.0")]
    [Description("RefQDeepCross is for testing")]
    public class RefQDeepCross : RustPlugin
    {
        [PluginReference]
        Plugin RefPDeepCross;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefPDeepCross != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefPDeepCross != null}");
                RefPDeepCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}