using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAJAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAJAssemblyOrder is for testing")]
    public class RefAJAssemblyOrder : RustPlugin
    {
        [PluginReference]
        Plugin RefAKAssemblyOrder;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAKAssemblyOrder != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAKAssemblyOrder != null}");
            RefAKAssemblyOrder?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}