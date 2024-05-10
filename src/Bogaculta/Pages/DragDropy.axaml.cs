using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

#pragma warning disable CS0618

namespace Bogaculta
{
    public partial class DragDropy : UserControl
    {
        public DragDropy()
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

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            if (e.Source is Control { Name: nameof(FileBox) })
                e.DragEffects &= DragDropEffects.Move;

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
                        FileInfo.Text = file.Name + " | " + file.Path;
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
    }
}