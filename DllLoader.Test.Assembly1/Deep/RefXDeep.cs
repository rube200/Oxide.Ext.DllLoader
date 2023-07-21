using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefXDee", "Rube200", "1.0.0")]
    [Description("RefXDeep is for testing")]
    public class RefXDeep : RustPlugin
    {
        [PluginReference]
        Plugin RefWDeep;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefWDeep != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefWDeep != null}");
            RefWDeep?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}