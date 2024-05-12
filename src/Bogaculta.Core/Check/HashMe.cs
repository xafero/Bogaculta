using System.Threading;
using System.Threading.Tasks;

namespace Bogaculta.Check
{
    public delegate Task<string> HashMe(CancellationToken token);
}