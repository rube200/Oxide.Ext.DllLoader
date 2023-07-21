using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefKCircular", "Rube200", "1.0.0")]
    [Description("RefKCircular is for testing")]
    public class RefKCircular : RustPlugin
    {
        [PluginReference]
        Plugin RefICircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefICircular != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefICircular != null}");
            RefICircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}