using System;
using System.IO;
using Bogaculta.Models;

namespace Bogaculta.IO
{
    internal static class FileTask
    {
        public static void DoMove(Job job)
        {
            var env = job.Env;
            var od = Path.GetFullPath(env.OutputDir);
            if (job.Source is FileInfo fi)
            {
                var srcFile = Path.GetFullPath(fi.FullName);
                var dstFile = Path.Combine(od, fi.Name);
                try
                {
                    if (!File.Exists(srcFile))
                        throw new IOException($"'{srcFile}' does not exist!");
                    if (File.Exists(dstFile))
                        throw new IOException($"'{dstFile}' already exists!");
                }
                catch (Exception e)
                {
                    job.Result = e.Message;
                }
            }
            else if (job.Source is DirectoryInfo di)
            {
                var srcDir = Path.GetFullPath(di.FullName);
                var dstDir = Path.Combine(od, di.Name);
                try
                {
                    if (!File.Exists(srcDir))
                        throw new IOException($"'{srcDir}' does not exist!");
                    if (File.Exists(dstDir))
                        throw new IOException($"'{dstDir}' already exists!");
                }
                catch (Exception e)
                {
                    job.Result = e.Message;
                }
            }
        }
    }
}