#region

using Oxide.Core.Plugins;

#endregion

namespace Oxide.Plugins
{
    [Info("RefAECircular", "Rube200", "1.0.0")]
    [Description("RefAECircular is for testing")]
    public class RefAECircular : RustPlugin
    {
        [PluginReference] private Plugin RefAFCircular;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefAFCircular != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefAFCircular != null}");
                RefAFCircular?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}