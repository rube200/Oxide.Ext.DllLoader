#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefAJAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAJAssemblyOrder is for testing")]
    public class RefAJAssemblyOrder : RustPlugin
    {
        [PluginReference] private Plugin RefAKAssemblyOrder;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAKAssemblyOrder != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefAKAssemblyOrder != null}");
                RefAKAssemblyOrder?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}