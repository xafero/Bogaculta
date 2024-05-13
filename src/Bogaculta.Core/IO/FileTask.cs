using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                    await MoveDir(od, di, job, token);
                }
                catch (Exception e)
                {
                    job.SetError(e.Message);
                }
            }
        }

        private static async Task MoveDir(string od, DirectoryInfo di, Job job,
            CancellationToken token)
        {
            var watch = new Stopwatch();
            watch.Start();

            var srcDir = Path.GetFullPath(di.FullName);
            var dstDir = Path.Combine(od, di.Name);
            dstDir = Path.GetFullPath(dstDir);

            if (!Directory.Exists(srcDir))
                throw new IOException($"'{srcDir}' does not exist!");
            if (Directory.Exists(dstDir))
                throw new IOException($"'{dstDir}' already exists!");

            Directory.CreateDirectory(dstDir);

            var count = await CopyDir(job, srcDir, dstDir, token);

            var srcDirI = new DirectoryInfo(srcDir);
            await HashTask.HashDir(job, srcDirI, token);

            var (_, aName) = HashTask.GetAlgo();
            var srcDirH = $"{srcDir}.{aName}";
            var dstDirH = $"{dstDir}.{aName}";
            await CopyFile(job, srcDirH, dstDirH, token);

            var dstDirI = new DirectoryInfo(dstDir);
            await HashTask.VerifyDir(job, dstDirI, token);

            watch.Stop();

            var t = string.Join("|", Enumerable.Repeat((bool?)true, count)
                .Select(x => x.GetText()[0]));
            var tmp = $"[{aName}] {t}";
            if (job.Result.Equals(tmp))
            {
                job.Result = $"Move took {watch.Elapsed.TotalSeconds} s!";
                Directory.Delete(srcDirI.FullName, recursive: true);
                File.Delete(srcDirH);
                return;
            }
            job.SetError("Move failed somehow!");
        }

        private static async Task<int> CopyDir(IJob job, string srcDir, string dstDir,
            CancellationToken token)
        {
            const string pattern = "*.*";
            const SearchOption opt = SearchOption.AllDirectories;
            var count = 0;
            foreach (var srcFile in Directory.EnumerateFiles(srcDir, pattern, opt))
            {
                var dstFileName = srcFile.Replace(srcDir, string.Empty)
                    .TrimStart('/', '\\');
                var dstFile = Path.Combine(dstDir, dstFileName);
                await CopyFile(job, srcFile, dstFile, token);
                count++;
            }
            return count;
        }

        private static async Task MoveFile(string od, FileInfo fi, Job job,
            CancellationToken token)
        {
            var watch = new Stopwatch();
            watch.Start();

            var srcFile = Path.GetFullPath(fi.FullName);
            var dstFile = Path.Combine(od, fi.Name);
            dstFile = Path.GetFullPath(dstFile);

            await CopyFile(job, srcFile, dstFile, token);

            var srcFileI = new FileInfo(srcFile);
            await HashTask.HashFile(job, srcFileI, token);

            var (_, aName) = HashTask.GetAlgo();
            var srcFileH = $"{srcFile}.{aName}";
            var dstFileH = $"{dstFile}.{aName}";
            await CopyFile(job, srcFileH, dstFileH, token);

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

        private static async Task CopyFile(IJob job, string srcFile, string dstFile,
            CancellationToken token)
        {
            if (!File.Exists(srcFile))
                throw new IOException($"'{srcFile}' does not exist!");
            if (File.Exists(dstFile))
                throw new IOException($"'{dstFile}' already exists!");

            await using var fInputR = File.OpenRead(srcFile);
            await using var fInput = fInputR.Count(job);
            await using var fOutputR = File.Create(dstFile!);
            await using var fOutput = fOutputR.Count(job);
            await fInput.CopyToAsync(fOutput, token);

            File.SetCreationTime(dstFile, File.GetCreationTime(srcFile));
            File.SetLastAccessTime(dstFile, File.GetLastAccessTime(srcFile));
            File.SetLastWriteTime(dstFile, File.GetLastWriteTime(srcFile));
        }
    }
}