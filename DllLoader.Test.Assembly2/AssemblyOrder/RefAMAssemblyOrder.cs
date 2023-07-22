#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefAMAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAMAssemblyOrder is for testing")]
    public class RefAMAssemblyOrder : RustPlugin
    {
        [PluginReference] private Plugin RefALAssemblyOrder;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefALAssemblyOrder != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefALAssemblyOrder != null}");
                RefALAssemblyOrder?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}