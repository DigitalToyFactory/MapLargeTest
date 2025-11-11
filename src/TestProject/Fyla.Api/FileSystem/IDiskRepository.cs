namespace Fyla.FileSystem
{
    public interface IDiskRepository
    {
        IEnumerable<DiskItem> GetDirectoryContents(string path);
    }
}
