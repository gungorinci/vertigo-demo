namespace VertigoDemo.Core
{
    public interface ISaveStore
    {
        string Load();

        void Save(string json);
    }
}
