namespace Fyla.FileSystem
{
    public interface IDiskRepository
    {
        IEnumerable<DiskItem> GetDirectoryContents(string path);
        IEnumerable<DiskItem> Search(string path, string pattern, bool deep);
        bool Delete(string path);
        bool Copy(string sourcePath, string destinationPath);
        bool Move(string sourcePath, string destinationPath);
    }
}
