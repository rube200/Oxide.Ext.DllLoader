namespace Oxide.Plugins
{
    [Info("RefAPAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefAPAssemblyDeep is for testing")]
    public class RefAPAssemblyDeep : RustPlugin
    {
        void Init()
        {
            var selfName = GetType().Name;
            Puts($"I am alive {selfName}");
        }

        private void CallRef(string callerMsg)
        {
            Puts($"CallRef says: '{callerMsg}'");
        }
    }
}