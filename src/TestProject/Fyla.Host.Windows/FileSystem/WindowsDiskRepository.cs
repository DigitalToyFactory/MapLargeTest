using Fyla.Helpers;
using Fyla.Orchestra;
using Microsoft.AspNetCore.Components.Web;
using System.IO;

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

        public IEnumerable<DiskItem> Search(string path, string pattern, bool deep)
        {
            var root = new DirectoryInfo(path);
            foreach(var fileObject in root.WalkTree(pattern, deep))
            {
                yield return DiskItem.From(fileObject);
            }
        }

        public bool Delete(string path)
        {            
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return true;
            }
            
            throw new FileNotFoundException();            
        }

        //TODO: move to Fyla.Helpers
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            }

            Directory.CreateDirectory(destDir);

            foreach (var file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                file.CopyTo(targetFilePath, overwrite: false);
            }

            foreach (var subDir in dir.GetDirectories())
            {
                string newDestDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestDir);
            }
        }

        public bool Copy(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                if (Directory.Exists(destinationPath) || string.IsNullOrEmpty(Path.GetExtension(destinationPath)))
                {
                    string fileName = Path.GetFileName(sourcePath);
                    destinationPath = Path.Combine(destinationPath, fileName);
                }

                File.Copy(sourcePath, destinationPath, overwrite: false);
                return true;
            }
            else if (Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, destinationPath);
                return true;
            }

            throw new FileNotFoundException($"Source not found: {sourcePath}");
        }

        public bool Move(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                if (Directory.Exists(destinationPath) || string.IsNullOrEmpty(Path.GetExtension(destinationPath)))
                {
                    string fileName = Path.GetFileName(sourcePath);
                    destinationPath = Path.Combine(destinationPath, fileName);
                }

                File.Move(sourcePath, destinationPath);
                return true;
            }
            else if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destinationPath);
                return true;
            }

            throw new FileNotFoundException($"Source not found: {sourcePath}");
        }

        public WindowsDiskRepository(IMaestro maestro)
        {
            _maestro = maestro;
            _maestro.Logger.Info("WindowsDiskRepository instantiated");            
        }
    }
}
