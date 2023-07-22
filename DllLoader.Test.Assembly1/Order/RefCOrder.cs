#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefCOrder", "Rube200", "1.0.0")]
    [Description("RefCOrder is for testing")]
    public class RefCOrder : RustPlugin
    {
        [PluginReference] private Plugin RefBOrder;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefBOrder != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefBOrder != null}");
                RefBOrder?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}