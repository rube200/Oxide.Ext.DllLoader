using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefANAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefANAssemblyDeep is for testing")]
    public class RefANAssemblyDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefAOAssemblyDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAOAssemblyDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAOAssemblyDeep != null}");
            RefAOAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}