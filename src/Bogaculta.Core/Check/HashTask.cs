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
                var fHashes = HashOneFile(algo, fi);
                var fHash = fHashes.Hash;
                job.Result = $"[{aName}] {fHash[..18]}";
            }
            else if (job.Source is DirectoryInfo di)
            {
                var dHashes = HashOneDir(algo, di);
                var dHash = string.Join("|", dHashes.Take(12).Select(d => d.Hash[..2]));
                job.Result = $"[{aName}] {dHash}";
            }
        }

        private static OneHash[] HashOneDir(HashAlgorithm algo, DirectoryInfo di)
        {
            var fullPath = di.FullName;
            const SearchOption opt = SearchOption.AllDirectories;
            const string pattern = "*.*";
            var files = Directory.EnumerateFiles(fullPath, pattern, opt)
                .Select(file => HashOneFile(algo, new FileInfo(file)))
                .ToArray();
            return files;
        }

        private static OneHash HashOneFile(HashAlgorithm algo, FileInfo fi)
        {
            var fullPath = fi.FullName;
            using var file = File.OpenRead(fullPath);
            var hash = algo.GetHash(file);
            return new OneHash(hash, fullPath);
        }
    }
}