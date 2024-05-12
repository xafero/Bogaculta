using System.IO;
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
                using var file = File.OpenRead(fi.FullName);
                var hash = algo.GetHash(file);
                job.Result = $"[{aName}] {hash}";
            }
        }
    }
}