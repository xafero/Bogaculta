using System.IO;
using Bogaculta.Proc;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bogaculta.Models
{
    public partial class Job : ObservableObject, IJob
    {
        public const string None = "<?>";

        [ObservableProperty] private FileSystemInfo _source;

        [ObservableProperty] private JobKind _kind;

        public string Parent => (Source as FileInfo)?.DirectoryName ??
                                (Source as DirectoryInfo)?.Parent?.FullName ?? None;

        public string Name => (Source as FileInfo)?.Name ??
                              (Source as DirectoryInfo)?.Name ?? None;

        [ObservableProperty] private string _worker;

        [ObservableProperty] private string _result;

        [ObservableProperty] private OneEnv _env;

        [ObservableProperty] private CountStream _tag;
    }
}