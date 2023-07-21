using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAMAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAMAssemblyOrder is for testing")]
    public class RefAMAssemblyOrder : RustPlugin
    {
        [PluginReference]
        Plugin RefALAssemblyOrder;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefALAssemblyOrder != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefALAssemblyOrder != null}");
            RefALAssemblyOrder?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}