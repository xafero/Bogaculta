using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Bogaculta.IO;
using Bogaculta.Models;
using Bogaculta.Proc;
using Bogaculta.Tools;
using Bogaculta.ViewModels;

#pragma warning disable CS0618

namespace Bogaculta.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private JobWorker? _worker;

        private void OnLoaded(object? _, RoutedEventArgs e)
        {
            HashMove.IsChecked = true;
            FileBox.AddHandler(DragDrop.DropEvent, OnDrop);
            FileBox.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            _worker = new JobWorker();
            _worker.Start();
        }

        private JobKind GetJobKind()
        {
            if (OnlyHash.IsChecked == true)
                return JobKind.Hash;

            if (OnlyVerify.IsChecked == true)
                return JobKind.Verify;

            if (HashMove.IsChecked == true)
                return JobKind.Move;

            return JobKind.Unspecified;
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Source is Control { Name: nameof(FileBox) })
                e.DragEffects &= DragDropEffects.Move;

            if (!e.Data.Contains(DataFormats.Text)
                && !e.Data.Contains(DataFormats.Files))
                e.DragEffects = DragDropEffects.None;
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            OnDrop(sender, new DragEventX(e));
        }

        private void OnDrop(object? _, DragEventX e)
        {
            if (e.Source is Control { Name: nameof(FileBox) })
                e.DragEffects &= DragDropEffects.Move;

            if (e.Data == null)
                return;

            if (e.Data.Contains(DataFormats.Text))
            {
                var text = e.Data.GetText();
                if (string.IsNullOrWhiteSpace(text))
                    return;
                var lines = text.Split('\n').Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l));
                foreach (var line in lines)
                    AddFileOrFolder(line);
                return;
            }

            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files == null)
                    return;
                foreach (var item in files)
                {
                    if (item is IStorageFile file)
                        AddFileOrFolder(file.Path);
                    else if (item is IStorageFolder folder)
                        AddFileOrFolder(folder.Path);
                }
                return;
            }

            if (e.Data.Contains(DataFormats.FileNames))
            {
                var files = e.Data.GetFileNames();
                if (files == null)
                    return;
                foreach (var file in files)
                    AddFileOrFolder(file);
            }
        }

        private IStorageProvider Sp => GetTopLevel(this)!.StorageProvider;

        private async void FileBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var data = new DataObject();

            var point = e.GetCurrentPoint(sender as Control);
            if (point.Properties.IsRightButtonPressed)
            {
                var folders = await Sp.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Choose input folder"
                });

                data.Set(DataFormats.Files, folders);
                OnDrop(sender, new DragEventX(sender, data, DragDropEffects.Move));
                return;
            }

            var files = await Sp.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose input file"
            });

            data.Set(DataFormats.Files, files);
            OnDrop(sender, new DragEventX(sender, data, DragDropEffects.Move));
        }

        private void Quit_OnClick(object? sender, RoutedEventArgs e)
        {
            Quit();
        }

        private void Quit(bool doClose = true)
        {
            _worker?.Stop();
            if (doClose)
                Close();
        }

        private void AddFileOrFolder(Uri uri)
            => AddFileOrFolder(uri.ToAbsolutePath());

        private void AddFileOrFolder(string path)
        {
            path = path.ToAbsolutePath();
            var env = new OneEnv(Model?.OutputFolder!);
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                var job = new Job { Source = dir, Env = env };
                Enlist(job);
            }
            else if (File.Exists(path))
            {
                var file = new FileInfo(path);
                var job = new Job { Source = file, Env = env };
                Enlist(job);
            }
        }

        private void Enlist(Job job)
        {
            if (job.Kind == JobKind.Unspecified)
                job.Kind = GetJobKind();
            _worker?.Enqueue(job);
            Model?.Jobs.Insert(0, job);
        }

        private MainWindowViewModel? Model => DataContext as MainWindowViewModel;

        private async void OutFolder_OnClick(object? sender, RoutedEventArgs e)
        {
            if (Model == null)
                return;
            var folders = await Sp.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Choose output folder", AllowMultiple = false
            });
            var folder = folders.SingleOrDefault();
            if (folder == null)
                return;
            Model.OutputFolder = folder.Path.ToAbsolutePath();
        }

        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            Quit(false);
        }
    }
}