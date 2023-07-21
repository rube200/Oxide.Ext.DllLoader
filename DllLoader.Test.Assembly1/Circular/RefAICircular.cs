using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefAICircular", "Rube200", "1.0.0")]
    [Description("RefAICircular is for testing")]
    public class RefAICircular : RustPlugin
    {
        [PluginReference]
        Plugin RefAGCircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAGCircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefAGCircular != null}");
            RefAGCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}