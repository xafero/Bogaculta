using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bogaculta.Check;
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
                    await MoveFile(od, fi, job, token);
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

        private static async Task MoveFile(string od, FileInfo fi, Job job,
            CancellationToken token)
        {
            var watch = new Stopwatch();
            watch.Start();

            var srcFile = Path.GetFullPath(fi.FullName);
            var dstFile = Path.Combine(od, fi.Name);
            dstFile = Path.GetFullPath(dstFile);

            await CopyFile(srcFile, dstFile, token);

            var srcFileI = new FileInfo(srcFile);
            await HashTask.HashFile(job, srcFileI, token);

            var (_, aName) = HashTask.GetAlgo();
            var srcFileH = $"{srcFile}.{aName}";
            var dstFileH = $"{dstFile}.{aName}";
            await CopyFile(srcFileH, dstFileH, token);

            var dstFileI = new FileInfo(dstFile);
            await HashTask.VerifyFile(job, dstFileI, token);

            watch.Stop();

            var tmp = $"[{aName}] {Strings.GetText(true)}";
            if (job.Result.Equals(tmp))
            {
                job.Result = $"Move took {watch.Elapsed.TotalSeconds} s!";
                File.Delete(srcFileI.FullName);
                File.Delete(srcFileH);
                return;
            }
            job.SetError("Move failed somehow!");
        }

        private static async Task CopyFile(string srcFile, string dstFile, CancellationToken token)
        {
            if (!File.Exists(srcFile))
                throw new IOException($"'{srcFile}' does not exist!");
            if (File.Exists(dstFile))
                throw new IOException($"'{dstFile}' already exists!");

            await using var fInput = File.OpenRead(srcFile);
            await using var fOutput = File.Create(dstFile!);
            await fInput.CopyToAsync(fOutput, token);

            File.SetCreationTime(dstFile, File.GetCreationTime(srcFile));
            File.SetLastAccessTime(dstFile, File.GetLastAccessTime(srcFile));
            File.SetLastWriteTime(dstFile, File.GetLastWriteTime(srcFile));
        }
    }
}