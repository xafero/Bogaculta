using Avalonia.Controls;
using Avalonia.Input;
using System;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Bogaculta.Tools;

#pragma warning disable CS0618

namespace Bogaculta
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
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

        private async void OnDrop(object? sender, DragEventX e)
        {
            if (e.Source is Control { Name: nameof(FileBox) })
                e.DragEffects &= DragDropEffects.Move;

            if (e.Data == null)
                return;

            if (e.Data.Contains(DataFormats.Text))
            {
                // TODO var text = e.Data.GetText();
            }
            else if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles() ?? Array.Empty<IStorageItem>();
                foreach (var item in files)
                {
                    if (item is IStorageFile file)
                    {
                        // FileInfo.Text = file.Name + " | " + file.Path;
                        // TODO Handle file
                    }
                    else if (item is IStorageFolder folder)
                    {
                        await foreach (var _ in folder.GetItemsAsync())
                        {
                            // TODO Handle Folder
                        }
                    }
                }
            }
            else if (e.Data.Contains(DataFormats.FileNames))
            {
                var files = e.Data.GetFileNames();
                // TODO Handle file names!
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
    }
}