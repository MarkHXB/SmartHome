namespace Saturn.BL.AppConfig
{
    public interface IConfigHandler
    {
        void Get(string keyName);
        void Set(string keyName);
        void Build();
    }
}
