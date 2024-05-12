using Bogaculta.Models;

namespace Bogaculta.Proc
{
    public static class JobTools
    {
        public static void SetError(this Job job, string text)
        {
            job.Result = $"[ERR] {text}";
        }
    }
}