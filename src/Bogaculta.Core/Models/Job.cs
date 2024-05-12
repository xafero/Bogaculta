using System.IO;

namespace Bogaculta.Models
{
    public sealed class Job
    {
        public const string None = "<?>";

        public FileSystemInfo Source { get; set; }

        public JobKind Kind { get; set; }

        public string Parent => (Source as FileInfo)?.DirectoryName ??
                                (Source as DirectoryInfo)?.Parent?.FullName ?? None;

        public string Name => (Source as FileInfo)?.Name ??
                              (Source as DirectoryInfo)?.Name ?? None;
    }
}