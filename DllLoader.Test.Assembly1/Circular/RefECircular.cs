using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefECircular", "Rube200", "1.0.0")]
    [Description("RefECircular is for testing")]
    public class RefECircular : RustPlugin
    {
        [PluginReference]
        Plugin RefFCircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefFCircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefFCircular != null}");
            RefFCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}