#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefNDeepCross", "Rube200", "1.0.0")]
    [Description("RefNDeepCross is for testing")]
    public class RefNDeepCross : RustPlugin
    {
        [PluginReference] private Plugin RefODeepCross;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefODeepCross != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefODeepCross != null}");
                RefODeepCross?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}