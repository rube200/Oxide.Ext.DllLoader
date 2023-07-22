#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefANAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefANAssemblyDeep is for testing")]
    public class RefANAssemblyDeep : RustPlugin
    {
        [PluginReference] private Plugin RefAOAssemblyDeep;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAOAssemblyDeep != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefAOAssemblyDeep != null}");
                RefAOAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}