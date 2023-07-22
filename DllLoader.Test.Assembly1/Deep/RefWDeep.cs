#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefWDeep", "Rube200", "1.0.0")]
    [Description("RefWDeep is for testing")]
    public class RefWDeep : RustPlugin
    {
        [PluginReference] private Plugin RefVDeep;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefVDeep != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefVDeep != null}");
                RefVDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}