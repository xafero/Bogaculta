using System.Threading;
using System.Threading.Tasks;

namespace Bogaculta.Check
{
    public sealed class LazyHash
    {
        private readonly HashMe _wrap;
        private string _hash;

        public LazyHash(HashMe wrap)
        {
            _wrap = wrap;
        }

        public LazyHash(string hash)
        {
            _hash = hash;
        }

        public async Task<string> Hash(CancellationToken token)
        {
            if (_hash != null)
                return _hash;
            var hash = await _wrap(token);
            return _hash = hash;
        }
    }
}