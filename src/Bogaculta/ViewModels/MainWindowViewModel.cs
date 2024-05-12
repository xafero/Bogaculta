using System.Collections.ObjectModel;
using Bogaculta.IO;
using Bogaculta.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bogaculta.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty] private string? _outputFolder;
        [ObservableProperty] private ObservableCollection<Job> _jobs;

        public MainWindowViewModel()
        {
            OutputFolder = Paths.GetLastDrive().FullName;
            Jobs = new ObservableCollection<Job>();
        }
    }
}