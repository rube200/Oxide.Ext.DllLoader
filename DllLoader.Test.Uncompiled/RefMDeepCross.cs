using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefMDeepCross", "Rube200", "1.0.0")]
    [Description("RefMDeepCross is for testing")]
    public class RefMDeepCross : RustPlugin
    {
        [PluginReference]
        Plugin RefNDeepCross;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefNDeepCross != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefNDeepCross != null}");
                RefNDeepCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}