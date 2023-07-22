#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefRDeepCross", "Rube200", "1.0.0")]
    [Description("RefRDeepCross is for testing")]
    public class RefRDeepCross : RustPlugin
    {
        [PluginReference] private Plugin RefQDeepCross;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefQDeepCross != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefQDeepCross != null}");
                RefQDeepCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}