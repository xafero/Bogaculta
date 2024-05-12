using System.Collections.Generic;

namespace Bogaculta.Check
{
    public record MultiHash(string Path, IEnumerable<OneHash> Hashes);
}