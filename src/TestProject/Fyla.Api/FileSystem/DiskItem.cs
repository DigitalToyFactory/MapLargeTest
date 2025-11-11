namespace Fyla.FileSystem
{
    public class DiskItem
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long? Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}
