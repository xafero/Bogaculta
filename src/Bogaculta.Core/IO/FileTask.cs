using System;
using System.IO;
using Bogaculta.Models;
using Bogaculta.Proc;

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
                dstFile = Path.GetFullPath(dstFile);
                try
                {
                    if (!File.Exists(srcFile))
                        throw new IOException($"'{srcFile}' does not exist!");
                    if (File.Exists(dstFile))
                        throw new IOException($"'{dstFile}' already exists!");
                    using var fs = File.Create(dstFile);
                    // TODO
                }
                catch (Exception e)
                {
                    job.SetError(e.Message);
                }
            }
            else if (job.Source is DirectoryInfo di)
            {
                var srcDir = Path.GetFullPath(di.FullName);
                var dstDir = Path.Combine(od, di.Name);
                dstDir = Path.GetFullPath(dstDir);
                try
                {
                    if (!Directory.Exists(srcDir))
                        throw new IOException($"'{srcDir}' does not exist!");
                    if (Directory.Exists(dstDir))
                        throw new IOException($"'{dstDir}' already exists!");
                    Directory.CreateDirectory(dstDir);
                    // TODO
                }
                catch (Exception e)
                {
                    job.SetError(e.Message);
                }
            }
        }
    }
}