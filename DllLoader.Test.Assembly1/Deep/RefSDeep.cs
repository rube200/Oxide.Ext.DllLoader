#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefSDeep", "Rube200", "1.0.0")]
    [Description("RefSDeep is for testing")]
    public class RefSDeep : RustPlugin
    {
        [PluginReference] private Plugin RefTDeep;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefTDeep != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefTDeep != null}");
                RefTDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}