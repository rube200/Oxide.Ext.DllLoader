#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefARAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefARAssemblyDeep is for testing")]
    public class RefARAssemblyDeep : RustPlugin
    {
        [PluginReference] private Plugin RefASAssemblyDeep;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefASAssemblyDeep != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefASAssemblyDeep != null}");
                RefASAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}