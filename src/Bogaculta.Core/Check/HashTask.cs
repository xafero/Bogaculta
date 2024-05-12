using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Bogaculta.IO;
using Bogaculta.Models;

namespace Bogaculta.Check
{
    internal static class HashTask
    {
        public static void DoHash(Job job)
        {
            using var algo = HashTool.GetHashAlgo();
            var aName = algo.GetTypeName();
            if (job.Source is FileInfo fi)
            {
                var fHash = HashOneFile(algo, fi);
                WriteHashFile(fHash.Path, aName, [fHash], false);
                job.Result = $"[{aName}] {fHash.Hash[..18]}";
            }
            else if (job.Source is DirectoryInfo di)
            {
                var dHash = HashOneDir(algo, di);
                WriteHashFile(dHash.Path, aName, dHash.Hashes, true);
                var debug = string.Join("|", dHash.Hashes.Take(12).Select(d => d.Hash[..2]));
                job.Result = $"[{aName}] {debug}";
            }
        }

        private static void WriteHashFile(string path, string ext, IEnumerable<OneHash> hashes,
            bool isDir, char mode = '*')
        {
            var rel = isDir ? Path.Combine(path, "_") : path;
            var tmp = $".{ext}";
            if (!path.EndsWith(tmp))
                path += tmp;
            using var writer = File.CreateText(path);
            writer.AutoFlush = true;
            writer.NewLine = "\n";
            foreach (var hash in hashes)
            {
                var local = Paths.GetRelative(rel, hash.Path);
                writer.WriteLine($"{hash.Hash} {mode}{local}");
            }
        }

        private static MultiHash HashOneDir(HashAlgorithm algo, DirectoryInfo di)
        {
            var fullPath = di.FullName;
            const SearchOption opt = SearchOption.AllDirectories;
            const string pattern = "*.*";
            var files = Directory.EnumerateFiles(fullPath, pattern, opt)
                .Select(file => HashOneFile(algo, new FileInfo(file)));
            return new MultiHash(fullPath, files);
        }

        private static OneHash HashOneFile(HashAlgorithm algo, FileInfo fi)
        {
            var fullPath = fi.FullName;
            using var file = File.OpenRead(fullPath);
            var hash = algo.GetHash(file);
            return new OneHash(fullPath, hash);
        }
    }
}