using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefFCircular", "Rube200", "1.0.0")]
    [Description("RefFCircular is for testing")]
    public class RefFCircular : RustPlugin
    {
        [PluginReference]
        Plugin RefECircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefECircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefECircular != null}");
            RefECircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}