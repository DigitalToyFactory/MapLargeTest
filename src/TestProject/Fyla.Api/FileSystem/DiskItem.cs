namespace Fyla.FileSystem
{
    public class DiskItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long? Size { get; set; }
        public DateTime LastModified { get; set; }

        public static DiskItem From(FileSystemInfo fileObject) 
            => fileObject is FileInfo fileInfo ? new DiskItem
            {
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                Type = "File",
                Size = fileInfo.Length,
                LastModified = fileInfo.LastWriteTime
            }
            : fileObject is DirectoryInfo dirInfo ? new DiskItem
            {
                Name = dirInfo.Name,
                Path = dirInfo.FullName,
                Type = "Directory",
                Size = 0,
                LastModified = dirInfo.LastWriteTime
            }
            : throw new NotSupportedException($"unknown FileSystemInfo type: {fileObject.GetType().FullName}");
    }
}
