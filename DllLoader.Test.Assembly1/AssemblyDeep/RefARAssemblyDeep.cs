using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefARAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefARAssemblyDeep is for testing")]
    public class RefARAssemblyDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefASAssemblyDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefASAssemblyDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefASAssemblyDeep != null}");
            RefASAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}