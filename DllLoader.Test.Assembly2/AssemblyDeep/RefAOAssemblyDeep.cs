using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAOAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAOAssemblyDeep is for testing")]
    public class RefAOAssemblyDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefAPAssemblyDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAPAssemblyDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAPAssemblyDeep != null}");
            RefAPAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}