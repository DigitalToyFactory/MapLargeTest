namespace Fyla.FileSystem
{
    public interface IDiskRepository
    {
        IEnumerable<DiskItem> GetDirectoryContents(string path);
        IEnumerable<DiskItem> Search(string path, string pattern, bool deep);
    }
}
