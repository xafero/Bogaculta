using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Bogaculta.Models;
using Bogaculta.Tools;

#pragma warning disable CS0618

namespace Bogaculta.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object? _, RoutedEventArgs e)
        {
            FileBox.AddHandler(DragDrop.DropEvent, OnDrop);
            FileBox.AddHandler(DragDrop.DragOverEvent, OnDragOver);
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

        private async void FileBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var sp = TopLevel.GetTopLevel(this)!.StorageProvider;
            var data = new DataObject();

            var point = e.GetCurrentPoint(sender as Control);
            if (point.Properties.IsRightButtonPressed)
            {
                var folders = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Choose folder"
                });

                data.Set(DataFormats.Files, folders);
                OnDrop(sender, new DragEventX(sender, data, DragDropEffects.Move));
                return;
            }

            var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose file"
            });

            data.Set(DataFormats.Files, files);
            OnDrop(sender, new DragEventX(sender, data, DragDropEffects.Move));
        }

        private void Quit_OnClick(object? sender, RoutedEventArgs e)
        {
            Quit();
        }

        private void Quit()
        {
            Close();
        }

        private void AddFileOrFolder(Uri uri)
        {
            var path = uri.AbsolutePath;
            AddFileOrFolder(path);
        }

        private void AddFileOrFolder(string path)
        {
            path = Path.GetFullPath(path);
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                var job = new Job();
                // TODO ?!
            }
            else if (File.Exists(path))
            {
                var file = new FileInfo(path);
                // TODO ?!
            }
        }
    }
}