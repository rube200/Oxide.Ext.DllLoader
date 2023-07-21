namespace Oxide.Plugins
{
    [Info("RefAKAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefAKAssemblyOrder is for testing")]
    public class RefAKAssemblyOrder : RustPlugin
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