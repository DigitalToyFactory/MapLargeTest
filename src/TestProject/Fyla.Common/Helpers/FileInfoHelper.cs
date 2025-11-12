using System.Security.Cryptography;

namespace Fyla.Helpers
{
    public static class FileInfoHelper
    {
        public static string CalculateFileMD5(this string filename)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string CalculateFileMD5(this FileInfo info) => CalculateFileMD5(info.FullName);
    }
}
