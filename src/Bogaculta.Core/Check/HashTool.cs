using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Bogaculta.Check
{
    internal static class HashTool
    {
        public static HashAlgorithm GetHashAlgo()
        {
            return SHA256.Create();
        }

        public static string GetHash(this HashAlgorithm algorithm, Stream stream)
        {
            var data = algorithm.ComputeHash(stream);
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