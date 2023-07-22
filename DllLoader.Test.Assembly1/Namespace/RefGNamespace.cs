#region

using Oxide.Core.Plugins;
using Oxide.Plugins;

#endregion

namespace Test.Namespace
{
    [Info("RefGNamespace", "Rube200", "1.0.0")]
    [Description("RefGNamespace is for testing")]
    public class RefGNamespace : RustPlugin
    {
        [PluginReference] private Plugin RefHNamespace;

        private void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefHNamespace != null}");
            timer.Once(3f, () =>
            {
                Puts($"Is Ref Loaded? (again) {RefHNamespace != null}");
                RefHNamespace?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
            });
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}