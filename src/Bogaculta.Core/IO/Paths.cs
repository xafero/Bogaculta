using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Bogaculta.IO
{
    public static class Paths
    {
        public static DirectoryInfo GetLastDrive()
        {
            var allDrives = DriveInfo.GetDrives();
            var lastDrive = allDrives.LastOrDefault();
            if (lastDrive != null)
                return lastDrive.RootDirectory;

            throw new InvalidOperationException(nameof(GetLastDrive));
        }

        public static string ToAbsolutePath(this Uri uri)
        {
            var path = uri.AbsolutePath;
            path = HttpUtility.UrlDecode(path);
            path = Path.GetFullPath(path);
            return path;
        }

        public static string ToAbsolutePath(this string path)
        {
            path = Path.GetFullPath(path);
            return path;
        }

        public static string GetRelative(string @ref, string path)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(@ref))!;
            var file = Path.GetFullPath(path);
            var @short = file.Replace(dir, string.Empty)
                .TrimStart('/', '\\');
            return @short;
        }

        public static string FindRelative(string @ref, string local)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(@ref))!;
            var file = Path.Combine(dir, local);
            return file;
        }
    }
}