using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RefJCircular", "Rube200", "1.0.0")]
    [Description("RefJCircular is for testing")]
    public class RefJCircular : RustPlugin
    {
        [PluginReference]
        Plugin RefKCircular;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefKCircular != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefKCircular != null}");
                RefKCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}