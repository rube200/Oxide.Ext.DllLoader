using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAQAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAQAssemblyDeep is for testing")]
    public class RefAQAssemblyDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefARAssemblyDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefARAssemblyDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefARAssemblyDeep != null}");
            RefARAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}