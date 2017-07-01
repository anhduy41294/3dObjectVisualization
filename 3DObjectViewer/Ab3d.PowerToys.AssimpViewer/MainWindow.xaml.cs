using Ab3d.Assimp;
using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ThreeDObjectViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AssimpWpfImporter _assimpWpfImporter;

        private string _lastFileName;

        private Dictionary<object, string> _objectNames;
        private Dictionary<string, object> _namedObjects;

        private Model3D _rootWpfModel3D;

        private object _currentlySelectedObject;

        private string _selectedObjectName;

        public MainWindow()
        {
            // IMPORTANT:
            // Native assimp library requires Visual C++ Redistributable for Visual Studio 2012 (only the following two files: msvcp110.dll and msvcr110.dll)
            // 
            // Probably the most proper way to ensure that the msvcp110.dll and msvcr110.dll files are available is to install the Visual C++ Redistributable for Visual Studio 2012
            // It can be installed from http://www.microsoft.com/en-gb/download/details.aspx?id=30679
            // In this case we only need to copy the Assimp32.dll and Assimp64.dll files to the same folder as executable file and Assimp.Net will automatically load the correct library.
            //
            // But if we do not want to install the redistributable, we can also manually copy the msvcp110.dll and msvcr110.dll to a location where the Assimp32.dll and Assimp64.dll are.
            // The problem is that both msvcp110.dll and msvcr110.dll also have 32 and 64 bit version with the same file name.
            // This means that we need to create two folders:
            // one for 32 bit and put there Assimp32.dll and 32 bit of msvcp110.dll and msvcr110.dll
            // and one for 64 bit and put there Assimp64.dll and 64bit of msvcp110.dll and msvcr110.dll
            // Then we need to tell Assimp.Net library where to find the correct native libraries.
            // This can be done with static AssimpWpfImporter.LoadAssimpNativeLibrary method (as shown below)
            //
            // Of couse, we can also simplify this and instead of compiling with AndCPU, compile with only x86 or only x64.
            // In this case we only need to ensure the the correct AssimpXX.dll and correct msvcp110.dll and msvcr110.dll are in the exe folder (no need to call AssimpWpfImporter.LoadAssimpNativeLibrary)

            string assimp32Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assimp32");
            string assimp64Folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assimp64");
            AssimpWpfImporter.LoadAssimpNativeLibrary(assimp32Folder, assimp64Folder);

            InitializeComponent();

            // This is not a final version
            var appVersion = this.GetType().Assembly.GetName().Version;

            ElementsTreeViewPanel1.SelectedElementChanged += ElementsTreeViewPanel_SelectedElementChanged;
            ElementsTreeViewPanel1.CameraCenterChanged += ElementsTreeViewPanel_CameraCenterChanged;
            ElementsTreeViewPanel1.InfoButtonClicked += ElementsTreeViewPanel_InfoButtonClicked;

            ObjectsPreviewPanel1.Model3DSelected += new EventHandler(ObjectsPreviewPanel_Model3DSelected);

            OptionsPanel1.CurrentObjectsPreviewPanel = ObjectsPreviewPanel1;

            InitAssimpImporter();

            var dragAndDropHelper = new DragAndDropHelper(this, "*");
            dragAndDropHelper.FileDroped += (sender, e) => ReadFile(e.FileName);
        }

        private void InitAssimpImporter()
        {
            _assimpWpfImporter = new AssimpWpfImporter();
            _assimpWpfImporter.LoggerCallback = LogMessage;
            _assimpWpfImporter.IsVerboseLoggingEnabled = true;

            _assimpWpfImporter.AssimpPostProcessSteps = PostProcessSteps.Triangulate;
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = "*";
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select a file with 3D models";
            openFileDialog.ValidateNames = true;

            if (_lastFileName != null)
                openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(_lastFileName);
            else
                openFileDialog.InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Ab3d.PowerToys.AssimpSample\Resources"));

            if (openFileDialog.ShowDialog() ?? false)
                ReadFile(openFileDialog.FileName);
        }

        private void ReadFile(string fileName, string texturesPath = null)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                ObjectsPreviewPanel1.ClearAll();

                ElementsTreeViewPanel1.ClearSelection();
                ElementsTreeViewPanel1.ClearTreeView();

                if (_assimpWpfImporter == null)
                    InitAssimpImporter();

                if (_assimpWpfImporter != null && _assimpWpfImporter.ImportedAssimpScene != null)
                    _assimpWpfImporter.ImportedAssimpScene.Clear();
                //_assimpWpfImporter.DisposeAssimpContext();

                

                _rootWpfModel3D = _assimpWpfImporter.ReadModel3D(fileName, texturesPath);

                if (_rootWpfModel3D == null)
                    return;

                // Direct access of the imported assimp Scene can be get from ImportedAssimpScene property
                var assimpScene = _assimpWpfImporter.ImportedAssimpScene;


                _namedObjects = _assimpWpfImporter.NamedObjects;

                // AssimpWpfImporter reads object names into NamedObjects dictionary where key is name and value is string (Dictionary<string, object>)
                // To get the names of objects we need to convert that into a dictionary where key is object and value is name
                _objectNames = FillObjectNames(_assimpWpfImporter.NamedObjects);


                // TODO: Check if we need to do any transparency sorting

                ObjectsPreviewPanel1.AddObjects(_rootWpfModel3D);
                ElementsTreeViewPanel1.FillTreeView(_rootWpfModel3D, _objectNames);

                ObjectsPreviewPanel1.IsCameraLightShown = true;
                ObjectsPreviewPanel1.SetSceneCamera();

                _lastFileName = fileName;

                // Show objects tab
                RootTabControl.SelectedItem = ObjectsTabItem;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importing file:\r\n" + ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        void ObjectsPreviewPanel_Model3DSelected(object sender, EventArgs e)
        {
            Model3D selectedModel3D = ObjectsPreviewPanel1.LastHitModel3D;

            if (selectedModel3D == null)
            {
                ObjectsPreviewPanel1.ClearSelection();
                ElementsTreeViewPanel1.ClearSelection();
                _selectedObjectName = null;
            }
            else
            {
                bool isSelectedModelFound = ElementsTreeViewPanel1.SelectTreeViewItem(selectedModel3D);

                // if we clicked on some wire grid or axis than isSelectedModelFound is false because the model is not found in TreeView
                if (isSelectedModelFound)
                    ObjectsPreviewPanel1.ShowSelectionLines(selectedModel3D);
            }
        }

        void ElementsTreeViewPanel_SelectedElementChanged(object sender, object selectedObject)
        {
            _currentlySelectedObject = selectedObject;

            if (selectedObject == null)
            {
                ObjectsPreviewPanel1.ClearSelection();
                _selectedObjectName = null;
            }
            else if (selectedObject is Model3D)
            {
                ObjectsPreviewPanel1.ShowSelectionLines(selectedObject as Model3D);

                if (!_objectNames.TryGetValue(selectedObject, out _selectedObjectName))
                    _selectedObjectName = null;
            }
        }


        void ElementsTreeViewPanel_CameraCenterChanged(object sender, object selectedObject)
        {
            if (selectedObject == null)
                ObjectsPreviewPanel1.SetSceneCamera();
            else
                ObjectsPreviewPanel1.SetObjectCamera(selectedObject as Model3D);
        }

        void ElementsTreeViewPanel_InfoButtonClicked(object sender, EventArgs e)
        {
            var infoWindow = new InfoWindow();

            infoWindow.NamedObjects = _namedObjects;
            infoWindow.ObjectNames = _objectNames;

            if (_currentlySelectedObject == null)
                infoWindow.SelectedModel = _rootWpfModel3D;
            else
                infoWindow.SelectedModel = _currentlySelectedObject as Model3D;

            string objectName;
            if (_objectNames != null && _currentlySelectedObject != null && _objectNames.TryGetValue(_currentlySelectedObject, out objectName))
                infoWindow.SelectedObjectName = objectName;
            else
                infoWindow.SelectedObjectName = "";

            infoWindow.ShowInTaskbar = false;
            infoWindow.Owner = this;

            infoWindow.ShowDialog();
        }


        private static Dictionary<object, string> FillObjectNames(Dictionary<string, object> namesObjects)
        {
            var objectNames = new Dictionary<object, string>();

            foreach (var oneName in namesObjects.Keys)
            {
                var oneObject = namesObjects[oneName];

                if (oneObject != null && !objectNames.ContainsKey(oneObject)) // it is possible that for example __AllLightsGroup == null
                    objectNames.Add(oneObject, oneName);
            }

            return objectNames;
        }

        private void LogMessage(string msg, string data = "")
        {
            // TODO:
            if (msg != null && msg.EndsWith("\n") && string.IsNullOrEmpty(data))
                System.Diagnostics.Debug.WriteLine(string.Format("\t{0}", msg));
            else
                System.Diagnostics.Debug.WriteLine(string.Format("\t{0}\t{1}\r\n", msg ?? "", data ?? ""));
        }

        private void ReloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastFileName))
                ReadFile(_lastFileName, null);
        }

        private void ClearAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            ObjectsPreviewPanel1.ClearAll();

            ElementsTreeViewPanel1.ClearSelection();
            ElementsTreeViewPanel1.ClearTreeView();

            _currentlySelectedObject = null;

            _namedObjects = null;
            _objectNames = null;
            _rootWpfModel3D = null;

            _assimpWpfImporter.Dispose();
            _assimpWpfImporter = null;

            GC.Collect();
            GC.WaitForFullGCComplete();
            GC.WaitForPendingFinalizers();
        }

        private void OnForceRightHandCoordinateSystemCheckBoxCheckedChanged(object sender, RoutedEventArgs e)
        {
            // Reload existing file
            if (!string.IsNullOrEmpty(_lastFileName))
                ReadFile(_lastFileName, null);
        }
    }
}
