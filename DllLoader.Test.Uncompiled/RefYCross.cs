using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefYCross", "Rube200", "1.0.0")]
    [Description("RefYCross is for testing")]
    public class RefYCross : RustPlugin
    {
        [PluginReference]
        Plugin RefZCross;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefZCross != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefZCross != null}");
                RefZCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}