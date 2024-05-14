using System.IO;
using Bogaculta.Models;
using Bogaculta.Proc;

namespace Bogaculta.IO
{
    public static class Counting
    {
        public static CountStream Count(this Stream stream, IJob job, Stream ctx = null)
        {
            var wrap = new CountStream(stream, ctx);
            job.Tag = wrap;
            return wrap;
        }
    }
}