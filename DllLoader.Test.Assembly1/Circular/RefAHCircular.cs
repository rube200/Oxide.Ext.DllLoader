using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAHCircular", "Rube200", "1.0.0")]
    [Description("RefAHCircular is for testing")]
    public class RefAHCircular : RustPlugin
    {
        [PluginReference]
        Plugin RefAICircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAICircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAICircular != null}");
            RefAICircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}