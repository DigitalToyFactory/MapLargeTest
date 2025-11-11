using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Fyla.Helpers
{
    public static class DirectoryInfoHelper
    {
        public static IEnumerable<FileSystemInfo> WalkTree(this DirectoryInfo root, string pattern, bool deep = false)
        {
            //root files
            foreach (var file in root.GetFiles(pattern))
            {
                yield return file;
            }

            //root directories
            foreach (var dir in root.GetDirectories(pattern))
            {
                yield return dir;
            }

            //recurse subdirectories (not just those matching the pattern)
            if(deep)
            {
                foreach (var dir in root.GetDirectories())
                {
                    foreach (var subItem in dir.WalkTree(pattern, deep))
                    {
                        yield return subItem;
                    }
                }
            }
        }
    }
}
