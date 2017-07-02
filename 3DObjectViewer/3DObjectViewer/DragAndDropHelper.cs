using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ThreeDObjectViewer
{
    public class FileDropedEventArgs : EventArgs
    {
        public string FileName;

        public FileDropedEventArgs(string fileName)
        {
            FileName = fileName;
        }
    }

    public class DragAndDropHelper
    {
        private FrameworkElement _parentToAddDragAndDrop;
        private string[] _allowedFileExtensions;

        public event EventHandler<FileDropedEventArgs> FileDroped;

        public DragAndDropHelper(FrameworkElement elementToAddDragAndDrop, string allowedFileExtensions)
        {
            _parentToAddDragAndDrop = elementToAddDragAndDrop;

            if (!string.IsNullOrEmpty(allowedFileExtensions))
                _allowedFileExtensions = allowedFileExtensions.Split(';');
            else
                _allowedFileExtensions = null;

            elementToAddDragAndDrop.AllowDrop = true;
            elementToAddDragAndDrop.Drop += new System.Windows.DragEventHandler(OnDragAndDrop_Drop);
            elementToAddDragAndDrop.DragOver += new System.Windows.DragEventHandler(OnDragAndDrop_DragOver);

        }

        public void OnDragAndDrop_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (e.Data.GetDataPresent("FileNameW"))
            {
                object dropData = e.Data.GetData("FileNameW");
                var dropFileNames = dropData as string[];

                if (dropFileNames != null && dropFileNames.Length > 0)
                {
                    if (_allowedFileExtensions == null || (_allowedFileExtensions.Length == 1 && _allowedFileExtensions[0] == "*"))
                    {
                        e.Effects = DragDropEffects.Move;
                        return;
                    }

                    string fileName = dropFileNames[0];
                    string fileExtension = System.IO.Path.GetExtension(fileName).ToLower();

                    foreach (string oneFileFilter in _allowedFileExtensions)
                    {
                        if (fileExtension == oneFileFilter)
                        {
                            e.Effects = DragDropEffects.Move;
                            break;
                        }
                    }
                }
            }
        }

        public void OnDragAndDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileNameW"))
            {
                object dropData = e.Data.GetData("FileNameW");

                var dropFileNames = dropData as string[];
                if (dropFileNames != null && dropFileNames.Length > 0)
                {
                    string fileName = dropFileNames[0];

                    if (FileDroped != null)
                        FileDroped(this, new FileDropedEventArgs(fileName));
                }
            }
        }
    }
}
