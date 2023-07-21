namespace Oxide.Plugins
{
    [Info("RefASAssemblyDeep", "Rube200", "1.0.0")]
    [Description("RefASAssemblyDeep is for testing")]
    public class RefASAssemblyDeep : RustPlugin
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