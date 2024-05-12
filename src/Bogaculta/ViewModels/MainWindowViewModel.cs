using Bogaculta.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bogaculta.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty] private string? _outputFolder;

        public MainWindowViewModel()
        {
            OutputFolder = Paths.GetLastDrive().FullName;
        }
    }
}