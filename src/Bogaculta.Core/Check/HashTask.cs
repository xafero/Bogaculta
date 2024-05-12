using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Bogaculta.IO;
using Bogaculta.Models;

namespace Bogaculta.Check
{
    internal static class HashTask
    {
        private static IEnumerable<OneHash> ReadHashFile(string path, string ext,
            bool isDir, char mode = '*')
        {
            var rel = isDir ? Path.Combine(path, "_") : path;
            var tmp = $".{ext}";
            if (!path.EndsWith(tmp))
                path += tmp;
            if (!File.Exists(path))
                yield break;
            using var reader = File.OpenText(path);
            while (reader.ReadLine() is { } line)
            {
                var parts = line.Trim().Split($" {mode}");
                var hash = parts.First().Trim();
                var local = parts.Last().Trim();
                var full = Paths.FindRelative(rel, local);
                yield return new OneHash(full, new LazyHash(hash));
            }
        }

        public static async Task DoVerify(Job job, CancellationToken token)
        {
            using var algo = HashTool.GetHashAlgo();
            var aName = algo.GetTypeName();
            if (job.Source is FileInfo fi)
            {
                var fVerified = default(bool?);
                var fHashes = ReadHashFile(fi.FullName, aName, false);
                var fHash = fHashes.SingleOrDefault();
                if (fHash != null)
                {
                    var single = await fHash.Lazy.Hash(token);
                    if (!string.IsNullOrWhiteSpace(single))
                    {
                        var newFHash = HashOneFile(algo, fi);
                        var newFHashH = await newFHash.Lazy.Hash(token);
                        fVerified = HashTool.VerifyHash(single, newFHashH);
                    }
                }
                job.Result = $"[{aName}] {fVerified.GetText()}";
            }
            else if (job.Source is DirectoryInfo di)
            {
                var newDHash = HashOneDir(algo, di);
                var dHashes = ReadHashFile(newDHash.Path, aName, true);
                var dVerified = new List<bool?>();
                foreach (var dItem in dHashes)
                {
                    var dFound = newDHash.Hashes.FirstOrDefault(x => x.Path == dItem.Path);
                    var dFirst = dFound == null ? null : await dFound.Lazy.Hash(token);
                    var dSecond = await dItem.Lazy.Hash(token);
                    dVerified.Add(HashTool.VerifyHash(dFirst, dSecond));
                }
                var debug = string.Join("|", dVerified.Select(d => d.GetText()[0]));
                job.Result = $"[{aName}] {debug}";
            }
        }

        public static async Task DoHash(Job job, CancellationToken token)
        {
            using var algo = HashTool.GetHashAlgo();
            var aName = algo.GetTypeName();
            if (job.Source is FileInfo fi)
            {
                var fHash = HashOneFile(algo, fi);
                await WriteHashFile(fHash.Path, aName, [fHash], false, token);
                var fItem = await fHash.Lazy.Hash(token);
                job.Result = $"[{aName}] {fItem[..18]}";
            }
            else if (job.Source is DirectoryInfo di)
            {
                var dHash = HashOneDir(algo, di);
                await WriteHashFile(dHash.Path, aName, dHash.Hashes, true, token);
                var dList = new List<string>();
                foreach (var dItem in dHash.Hashes.Take(12))
                    dList.Add((await dItem.Lazy.Hash(token))[..2]);
                job.Result = $"[{aName}] {string.Join("|", dList)}";
            }
        }

        private static async Task WriteHashFile(string path, string ext, IEnumerable<OneHash> hashes,
            bool isDir, CancellationToken token, char mode = '*')
        {
            var rel = isDir ? Path.Combine(path, "_") : path;
            var tmp = $".{ext}";
            if (!path.EndsWith(tmp))
                path += tmp;
            await using var writer = File.CreateText(path);
            writer.AutoFlush = true;
            writer.NewLine = "\n";
            foreach (var hash in hashes)
            {
                var local = Paths.GetRelative(rel, hash.Path);
                var hashed = await hash.Lazy.Hash(token);
                await writer.WriteLineAsync($"{hashed} {mode}{local}");
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
            return new OneHash(fullPath, new LazyHash(async token =>
            {
                await using var file = File.OpenRead(fullPath);
                var hash = await algo.GetHash(file, token);
                return hash;
            }));
        }
    }
}