using System.IO;
using Bogaculta.Models;
using Bogaculta.Proc;

namespace Bogaculta.IO
{
    public static class Counting
    {
        public static CountStream Count(this Stream stream, IJob job)
        {
            var wrap = new CountStream(stream);
            job.Tag = wrap;
            return wrap;
        }
    }
}