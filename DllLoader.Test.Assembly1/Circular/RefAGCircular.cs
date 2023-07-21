using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAGCircular", "Rube200", "1.0.0")]
    [Description("RefAGCircular is for testing")]
    public class RefAGCircular : RustPlugin
    {
        [PluginReference]
        Plugin RefAHCircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAHCircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAHCircular != null}");
            RefAHCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}