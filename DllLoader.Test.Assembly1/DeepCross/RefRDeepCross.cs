using Oxide.Core.Libraries;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefRDeepCross", "Rube200", "1.0.0")]
    [Description("RefRDeepCross is for testing")]
    public class RefRDeepCross : RustPlugin
    {
        [PluginReference]
        Plugin RefQDeepCross;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefQDeepCross != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefQDeepCross != null}");
            RefQDeepCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}