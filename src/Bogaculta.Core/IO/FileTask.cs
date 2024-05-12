using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bogaculta.Models;
using Bogaculta.Proc;

namespace Bogaculta.IO
{
    internal static class FileTask
    {
        public static async Task DoMove(Job job, CancellationToken token)
        {
            var env = job.Env;
            var od = Path.GetFullPath(env.OutputDir);
            if (job.Source is FileInfo fi)
            {
                try
                {
                    await MoveFile(od, fi, token);
                }
                catch (Exception e)
                {
                    job.SetError(e.Message);
                }
            }
            else if (job.Source is DirectoryInfo di)
            {
                try
                {
                    await MoveDir(od, di, token);
                }
                catch (Exception e)
                {
                    job.SetError(e.Message);
                }
            }
        }

        private static async Task MoveDir(string od, DirectoryInfo di, CancellationToken token)
        {
            var srcDir = Path.GetFullPath(di.FullName);
            var dstDir = Path.Combine(od, di.Name);
            dstDir = Path.GetFullPath(dstDir);
            if (!Directory.Exists(srcDir))
                throw new IOException($"'{srcDir}' does not exist!");
            if (Directory.Exists(dstDir))
                throw new IOException($"'{dstDir}' already exists!");
            Directory.CreateDirectory(dstDir);

            // TODO ?!
        }

        private static async Task MoveFile(string od, FileInfo fi, CancellationToken token)
        {
            var srcFile = Path.GetFullPath(fi.FullName);
            var dstFile = Path.Combine(od, fi.Name);
            dstFile = Path.GetFullPath(dstFile);
            if (!File.Exists(srcFile))
                throw new IOException($"'{srcFile}' does not exist!");
            if (File.Exists(dstFile))
                throw new IOException($"'{dstFile}' already exists!");
            await using var fInput = File.OpenRead(srcFile);
            await using var fOutput = File.Create(dstFile);
            await fInput.CopyToAsync(fOutput, token);

            // TODO ?!
        }
    }
}