#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefTDeep", "Rube200", "1.0.0")]
    [Description("RefTDeep is for testing")]
    public class RefTDeep : RustPlugin
    {
        [PluginReference] private Plugin RefUDeep;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefUDeep != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefUDeep != null}");
                RefUDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}