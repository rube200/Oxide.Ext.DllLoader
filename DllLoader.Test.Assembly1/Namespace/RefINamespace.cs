using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Plugins;

namespace Test.Namespace
{
    [Info("RefINamespace", "Rube200", "1.0.0")]
    [Description("RefINamespace is for testing")]
    public class RefINamespace : RustPlugin
    {
        [PluginReference]
        Plugin RefHNamespace;

        void Init()
        {
            var selfName = GetType().Name;

            Puts($"I am alive {selfName}");
            Puts($"Is Ref Loaded? {RefHNamespace != null}");
        }

        void Loaded()
        {
            Puts($"Is Ref Loaded? (again) {RefHNamespace != null}");
            RefHNamespace?.Call(nameof(CallRef), $"Hello this is {GetType().Name}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}