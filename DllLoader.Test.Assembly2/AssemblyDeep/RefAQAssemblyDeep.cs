#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefAQAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAQAssemblyDeep is for testing")]
    public class RefAQAssemblyDeep : RustPlugin
    {
        [PluginReference] private Plugin RefARAssemblyDeep;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefARAssemblyDeep != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefARAssemblyDeep != null}");
                RefARAssemblyDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}