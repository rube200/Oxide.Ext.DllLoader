namespace Oxide.Ext.DllLoader.API
{
    public interface IDllLoaderMapperLoadable : IDllLoaderMapper
    {
        void OnModLoad();

        void OnShutdown();
    }
}