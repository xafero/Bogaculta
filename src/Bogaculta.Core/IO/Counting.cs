using System.IO;
using Bogaculta.Proc;

namespace Bogaculta.IO
{
    public static class Counting
    {
        public static CountStream Count(this Stream stream)
        {
            var wrap = new CountStream(stream);
            return wrap;
        }
    }
}