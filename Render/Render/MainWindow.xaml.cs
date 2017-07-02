using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Ab3d.Common.Cameras;
using Ab3d.DirectX;
using Ab3d.DirectX.Effects;
using Ab3d.DirectX.Materials;
using Ab3d.Visuals;
using SharpDX;
using SharpDX.Direct3D;

namespace Render
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const bool _isUsingPixelsVisual3D = true; // Set to false to use low lever DXEngine objects (MeshObjectNode and PixelMaterial) instead of PixelsVisual3D
        private DisposeList _modelDisposables;
        private PixelEffect _pixelEffect;
        private GeometryModel3D _selectedModel;

        public MainWindow()
        {
            InitializeComponent();

            _modelDisposables = new DisposeList();

            MainDXViewportView.DXSceneInitialized += delegate (object sender, EventArgs args)
            {
                CreateScene();
            };

            this.Unloaded += delegate (object sender, RoutedEventArgs args)
            {
                _modelDisposables.Dispose();

                if (_pixelEffect != null)
                {
                    _pixelEffect.Dispose();
                    _pixelEffect = null;
                }

                MainDXViewportView.Dispose();
            };
        }

        private string _lastFileName;

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
            var readerObj = new Ab3d.ReaderObj();
            var readModel3D = readerObj.ReadModel3D(fileName) as GeometryModel3D;
            var transform3DGroup = new Transform3DGroup();
            transform3DGroup.Children.Add(new ScaleTransform3D(1000, 1000, 1000));
            transform3DGroup.Children.Add(new TranslateTransform3D(0, -120, 0));

            readModel3D.Transform = transform3DGroup;

            Ab3d.Utilities.ModelUtils.ChangeMaterial(readModel3D, newMaterial: new DiffuseMaterial(Brushes.Red), newBackMaterial: null);

            float pixelSize = 2;
            ShowGeometryModel3D(readModel3D, pixelSize);

            _selectedModel = readModel3D;
            _lastFileName = fileName;
            CenterButton.IsEnabled = true;
        }

        private void CreateScene()
        {
            if (MainDXViewportView.DXScene == null)
                return; // Not yet initialized or using WPF 3D

            Mouse.OverrideCursor = Cursors.Wait;

            MainViewport.Children.Clear();
            _modelDisposables.Dispose(); // Dispose previously used resources

            _modelDisposables = new DisposeList(); // Start with a fresh DisposeList

            Mouse.OverrideCursor = null;
        }

        private void ShowPositionsArray(Vector3[] positionsArray, float pixelSize, Color4 pixelColor, Bounds positionBounds)
        {
            // The easiest way to show many pixels is to use PixelsVisual3D.
            var pixelsVisual3D = new PixelsVisual3D()
            {
                Positions = positionsArray,
                PixelColor = pixelColor.ToWpfColor(),
                PixelSize = pixelSize
            };
            
            pixelsVisual3D.PositionsBounds = positionBounds;

            MainViewport.Children.Add(pixelsVisual3D);
        }

        private void ShowGeometryModel3D(GeometryModel3D model3D, float pixelSize)
        {
            MainViewport.Children.Clear();
            _modelDisposables.Dispose(); // Dispose previously used resources

            _modelDisposables = new DisposeList(); // Start with a fresh DisposeList

            if (_pixelEffect == null)
            {
                _pixelEffect = MainDXViewportView.DXScene.DXDevice.EffectsManager.GetEffect<PixelEffect>(createNewEffectInstanceIfNotFound: true);
            }

            _pixelEffect.PixelSize = pixelSize;

            // To override the used material, we first need to create a new WpfMaterial from the WPF material.
            var wpfMaterial = new WpfMaterial(model3D.Material);

            // then set the Effect to it ...
            wpfMaterial.Effect = _pixelEffect;

            // and finally specify the WpfMaterial to be used whenever the model3D.Material is used.
            model3D.Material.SetUsedDXMaterial(wpfMaterial);

            _modelDisposables.Add(wpfMaterial);


            // Now just add the model3D to the MainViewport
            var modelVisual3D = new ModelVisual3D();
            modelVisual3D.Content = model3D;

            MainViewport.Children.Add(modelVisual3D);
        }

        private void OnSceneTypeChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded || DesignerProperties.GetIsInDesignMode(this))
                return;

            CreateScene();
        }

        private Vector3[] CreatePositionsArray(Point3D center, Size3D size, int xCount, int yCount, int zCount)
        {
            var positionsArray = new Vector3[xCount * yCount * zCount];

            float xStep = (float)(size.X / xCount);
            float yStep = (float)(size.Y / yCount);
            float zStep = (float)(size.Z / zCount);

            int i = 0;
            for (int z = 0; z < zCount; z++)
            {
                float zPos = (float)(center.Z - (size.Z / 2.0) + (z * zStep));

                for (int y = 0; y < yCount; y++)
                {
                    float yPos = (float)(center.Y - (size.Y / 2.0) + (y * yStep));

                    for (int x = 0; x < xCount; x++)
                    {
                        float xPos = (float)(center.X - (size.X / 2.0) + (x * xStep));

                        positionsArray[i] = new Vector3(xPos, yPos, zPos);
                        i++;
                    }
                }
            }

            return positionsArray;
        }

        private class CameraSettings
        {
            public double Heading, Attitude, Bank;
            public double Distance;

            public CameraSettings()
            {
                Heading = 30;
                Attitude = -20;
                Bank = 0;

                Distance = 100;
            }
        }

        private CameraSettings _sceneCameraSettings;

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            Point3D center;
            double size;
            GetModelCenterAndSize(_selectedModel, out center, out size);

            if (_sceneCameraSettings == null)
                _sceneCameraSettings = new CameraSettings();
            _sceneCameraSettings.Distance = size * 2;

            Camera1.BeginInit();

            SetTargetPositionCamera(_sceneCameraSettings);
            Camera1.TargetPosition = center;
            SetCameraCommonSettings();

            Camera1.EndInit();

            // In case all the previosusly shown objects were cleared from Viewer3ds, this also removed the camera light. 
            // To regenerate the light in this case, we need to call the Refresh method
            Camera1.Refresh();
        }

        private void SetTargetPositionCamera(CameraSettings settings)
        {
            Camera1.Offset = new Vector3D(); // Reset the offset

            Camera1.Distance = settings.Distance;

            Camera1.Heading = settings.Heading;
            Camera1.Attitude = settings.Attitude;
            Camera1.Bank = settings.Bank;
        }

        private void SetCameraCommonSettings()
        {
            Camera1.NearPlaneDistance = 0.125;
            Camera1.FarPlaneDistance = double.PositiveInfinity;
            Camera1.FieldOfView = 45;
        }

        private void GetModelCenterAndSize(Model3D model, out Point3D center, out double size)
        {
            Rect3D bounds;
            bounds = model.Bounds;

            center = new Point3D(bounds.X + bounds.SizeX / 2,
                                 bounds.Y + bounds.SizeY / 2,
                                 bounds.Z + bounds.SizeZ / 2);

            size = Math.Sqrt(bounds.SizeX * bounds.SizeX +
                             bounds.SizeY * bounds.SizeY +
                             bounds.SizeZ * bounds.SizeZ);
        }
    }
}
