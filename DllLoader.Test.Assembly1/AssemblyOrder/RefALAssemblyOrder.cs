namespace Oxide.Plugins
{
    [Info("RefALAssemblyOrder", "Rube200", "1.0.0")]
    [Description("RefALAssemblyOrder is for testing")]
    public class RefALAssemblyOrder : RustPlugin
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