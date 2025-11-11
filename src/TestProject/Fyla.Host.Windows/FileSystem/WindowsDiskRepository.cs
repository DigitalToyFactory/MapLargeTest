using Fyla.Orchestra;

namespace Fyla.FileSystem
{
    public class WindowsDiskRepository : IDiskRepository
    {
        private readonly IMaestro _maestro;

        public IEnumerable<DiskItem> GetDirectoryContents(string path)
        {
            var dirInfo = new DirectoryInfo(path);

            var dirs = dirInfo.GetDirectories().Select(d => new DiskItem
            {
                Name = d.Name,
                Type = "Directory",
                Size = null,
                LastModified = d.LastWriteTime
            });

            var files = dirInfo.GetFiles().Select(f => new DiskItem
            {
                Name = f.Name,
                Type = "File",
                Size = f.Length,
                LastModified = f.LastWriteTime
            });

            return dirs.Concat(files);
        }
    
        public WindowsDiskRepository(IMaestro maestro)
        {
            _maestro = maestro;
            _maestro.Logger.Info("WindowsDiskRepository instantiated");            
        }
    }
}
