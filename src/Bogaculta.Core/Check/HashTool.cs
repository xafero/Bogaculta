using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogaculta.IO;

namespace Bogaculta.Check
{
    internal static class HashTool
    {
        public static HashAlgorithm GetHashAlgo()
        {
            return SHA256.Create();
        }

        public static async Task<string> GetHash(this HashAlgorithm algorithm, Stream stream,
            CancellationToken token)
        {
            var count = stream.Count();
            var data = await algorithm.ComputeHashAsync(count, token);
            var builder = new StringBuilder();
            foreach (var @byte in data)
                builder.Append(@byte.ToString("x2"));
            return builder.ToString();
        }

        public static bool? VerifyHash(string reHashed, string hash)
        {
            if (reHashed == null || hash == null)
                return null;
            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(reHashed, hash) == 0;
        }
    }
}