using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using Ab3d.Controls;
using Ab3d.Visuals;
using Ab3d.Utilities;

namespace ThreeDObjectViewer
{
    /// <summary>
    /// Interaction logic for ObjectsPreviewPanel.xaml
    /// </summary>
    public partial class ObjectsPreviewPanel : UserControl
    {
        private bool _isUsingCustomCamera;
        private bool _isCameraLightShown;

        private Model3D _rootModel3D;
        private bool _isCameraChangedInternally;

        private Model3D _savedCenterObject;
        private CameraSettings _savedCameraSettings;
        private Camera _savedWpfCamera;

        private WireGridVisual3D _wireGrid;
        private ColoredAxisVisual3D _coloredAxisVisual3D;
        
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
        private CameraSettings _objectCameraSettings;


        public event EventHandler CameraChanged;

        public bool IsCameraLightShown
        {
            get { return _isCameraLightShown; }
            set 
            { 
                _isCameraLightShown = value;
                UpateCameraLight();
            }
        }


        public bool IsCameraAnimating { get; private set; }



        public Ab3d.Visuals.WireGridVisual3D WireGrid
        {
            get
            {
                return _wireGrid;
            }
        }

        public Ab3d.Visuals.ColoredAxisVisual3D BigAxis
        {
            get
            {
                return _coloredAxisVisual3D;
            }
        }

        public bool IsCameraAxisShown
        {
            get { return (CameraAxisPanel1.Visibility == System.Windows.Visibility.Visible); }
            set 
            {
                if (value)
                    CameraAxisPanel1.Visibility = System.Windows.Visibility.Visible;
                else
                    CameraAxisPanel1.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public bool IsCameraPreviewShown
        {
            get { return (CameraPreviewBorder.Visibility == System.Windows.Visibility.Visible); }
            set
            {
                if (value)
                    CameraPreviewBorder.Visibility = System.Windows.Visibility.Visible;
                else
                    CameraPreviewBorder.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private bool _selectionShowBoundingBox;

        public bool SelectionShowBoundingBox
        {
            get { return _selectionShowBoundingBox; }
            set
            {
                if (_selectionShowBoundingBox == value)
                    return;

                _selectionShowBoundingBox = value;

                if (SelectedModelDecorator != null)
                    SelectedModelDecorator.ShowBoundingBox = _selectionShowBoundingBox;
            }
        }


        private bool _selectionShowNormals;

        public bool SelectionShowNormals
        {
            get { return _selectionShowNormals; }
            set
            {
                if (_selectionShowNormals == value)
                    return;

                _selectionShowNormals = value;

                if (SelectedModelDecorator != null)
                    SelectedModelDecorator.ShowNormals = _selectionShowNormals;
            }
        }


        private bool _selectionShowTriangles;

        public bool SelectionShowTriangles
        {
            get { return _selectionShowTriangles; }
            set
            {
                if (_selectionShowTriangles == value)
                    return;

                _selectionShowTriangles = value;

                if (SelectedModelDecorator != null)
                    SelectedModelDecorator.ShowTriangles = _selectionShowTriangles;
            }
        }
        
        public bool IsWireGridShown { get; set; }
        public bool IsBigAxisShown { get; set; }


        public Viewport3D Viewport3d
        {
            get { return MainViewport3D; }
        }

        public Ab3d.Cameras.TargetPositionCamera UsedTargetPositionCamera
        {
            get { return TargetPositionCamera1; }
        }


        private System.Windows.Threading.DispatcherTimer _keyboardCheckTimer;

        public ObjectsPreviewPanel()
        {
            InitializeComponent();

            CameraControllerInfo.AddCustomInfoLine(0, MouseCameraController.MouseAndKeyboardConditions.LeftMouseButtonPressed, "Select object");

            SelectionShowBoundingBox = true;

            this.MouseLeftButtonDown += new MouseButtonEventHandler(ObjectsPreviewPanel_MouseLeftButtonDown);
        }

        /// <summary>
        /// Shows wires and axis
        /// </summary>
        public void ShowAdditionalVisualElements()
        {
            ShowWireGrid();
            ShowBigAxis();
        }

        public void ShowWireGrid()
        {
            if (!IsWireGridShown)
                return;

            if (_wireGrid == null)
            {
                _wireGrid = new WireGridVisual3D();
                _wireGrid.BeginInit();
                _wireGrid.HeightCellsCount = 10;
                _wireGrid.WidthCellsCount = 10;
                _wireGrid.LineColor = Colors.Gray;
                _wireGrid.LineThickness = 1;
                _wireGrid.EndInit();
            }

            if (!MainViewport3D.Children.Contains(_wireGrid))
                MainViewport3D.Children.Add(_wireGrid);
        }

        public void ShowBigAxis()
        {
            if (!IsBigAxisShown)
                return;

            if (_coloredAxisVisual3D == null)
            {
                _coloredAxisVisual3D = new ColoredAxisVisual3D();
                _coloredAxisVisual3D.LineThickness = 3;
            }

            if (!MainViewport3D.Children.Contains(_coloredAxisVisual3D))
                MainViewport3D.Children.Add(_coloredAxisVisual3D);
        }

        /// <summary>
        /// Hide wires and axis
        /// </summary>
        public void HideAdditionalVisualElements()
        {
            HideWireGrid();
            HideBigAxis();
        }

        public void HideWireGrid()
        {
            if (_wireGrid != null && MainViewport3D.Children.Contains(_wireGrid))
                MainViewport3D.Children.Remove(_wireGrid);
        }

        public void HideBigAxis()
        {
            if (_coloredAxisVisual3D != null && MainViewport3D.Children.Contains(_coloredAxisVisual3D))
                MainViewport3D.Children.Remove(_coloredAxisVisual3D);
        }

        public void AddObjects(Model3D modelsToAdd)
        {
            if (_rootModel3D == null)
                _rootModel3D = modelsToAdd;

            ObjectsRoot.Children.Add(modelsToAdd);


            if (!CameraControlPanels.IsEnabled)
                CameraControlPanels.IsEnabled = true;

            if (!MouseCameraController1.IsEnabled)
                MouseCameraController1.IsEnabled = true;
        }

        public void ShowEmptySceneCamera()
        {
            double maxSize = 0;

            if (_wireGrid != null)
                maxSize = _wireGrid.Size.Width;

            if (_coloredAxisVisual3D != null && _coloredAxisVisual3D.Length > maxSize)
                maxSize = _coloredAxisVisual3D.Length;

            if (maxSize == 0)
                maxSize = 100;

            maxSize *= 2;

            TargetPositionCamera1.Distance = maxSize;
            TargetPositionCamera1.Refresh();

            TargetPositionCamera1.IsEnabled = true;
            CameraControlPanels.IsEnabled = true;
            MouseCameraController1.IsEnabled = true;
        }

        public void AddLight(Light lightToAdd)
        {
            if (!LightsRoot.Children.Contains(lightToAdd))
                LightsRoot.Children.Add(lightToAdd);
        }

        public void RemoveLight(Light lightToRemove)
        {
            if (LightsRoot.Children.Contains(lightToRemove))
                LightsRoot.Children.Remove(lightToRemove);
        }

        public void ClearObjects()
        {
            _rootModel3D = null;
            _savedCenterObject = null;
            ObjectsRoot.Children.Clear();
        }

        public void ClearLights()
        {
            LightsRoot.Children.Clear();
        }

        public void ClearAll()
        {
            ClearObjects();
            ClearLights();
        }

        public void StartAnimatingCamera(Camera cameraToAnimate)
        {
            _isUsingCustomCamera = false;
            IsCameraAnimating = true;

            TargetPositionCamera1.IsEnabled = false;
            MainViewport3D.Camera = cameraToAnimate;
        }

        public void UpdateAnimatedCamera(Camera animatedCamera)
        {
            if (IsCameraAnimating)
                MainViewport3D.Camera = animatedCamera;
        }

        public void EndAnimatingCamera()
        {
            if (!IsCameraAnimating)
                return;

            SetTargetPositionCameraFromWpfCamera(MainViewport3D.Camera);
            TargetPositionCamera1.IsEnabled = true;
            IsCameraAnimating = false;
        }

        public void SetCamera(Camera newCamera)
        {
            SetTargetPositionCameraFromWpfCamera(newCamera);

            _savedWpfCamera = newCamera;

            _isUsingCustomCamera = false;
        }

        public void SetSceneCamera()
        {
            if (_sceneCameraSettings == null)
                _sceneCameraSettings = new CameraSettings();

            SetObjectCamera(_rootModel3D, _sceneCameraSettings);

            _isUsingCustomCamera = true;
        }

        public void SetObjectCamera(Model3D centerObject)
        {
            if (_objectCameraSettings == null)
                _objectCameraSettings = new CameraSettings();

            SetObjectCamera(centerObject, _objectCameraSettings);

            _isUsingCustomCamera = true;
        }

        private void SetObjectCamera(Model3D centerObject, CameraSettings cameraSettings)
        {
            Point3D center;
            double size;

            _savedWpfCamera = null;

            if (_savedCenterObject == null)
            {
                _savedCenterObject = centerObject;
                _savedCameraSettings = cameraSettings;
            }

            if (centerObject == null)
            {
                center = new Point3D();
                size = 100;
            }
            else
            {
                GetModelCenterAndSize(centerObject, out center, out size);

                if (double.IsNaN(center.X))
                    center = new Point3D();

                if (double.IsInfinity(size))
                    size = 1;
            }

            cameraSettings.Distance = size * 2;

            TargetPositionCamera1.BeginInit();

            SetTargetPositionCamera(cameraSettings);
            TargetPositionCamera1.TargetPosition = center;
            SetCameraCommonSettings();

            TargetPositionCamera1.EndInit();

            // In case all the previosusly shown objects were cleared from Viewer3ds, this also removed the camera light. 
            // To regenerate the light in this case, we need to call the Refresh method
            TargetPositionCamera1.Refresh();
        }

        private void SetTargetPositionCamera(CameraSettings settings)
        {
            TargetPositionCamera1.Offset = new Vector3D(); // Reset the offset

            TargetPositionCamera1.Distance = settings.Distance;

            TargetPositionCamera1.Heading = settings.Heading;
            TargetPositionCamera1.Attitude = settings.Attitude;
            TargetPositionCamera1.Bank = settings.Bank;
        }

        private void SetCameraCommonSettings()
        {
            TargetPositionCamera1.NearPlaneDistance = 0.125;
            TargetPositionCamera1.FarPlaneDistance = double.PositiveInfinity;
            TargetPositionCamera1.FieldOfView = 45;
        }

        private void TargetPositionCamera1_CameraChanged(object sender, Ab3d.Common.Cameras.CameraChangedRoutedEventArgs e)
        {
            if (!_isCameraChangedInternally && CameraChanged != null)
                CameraChanged(this, null);
        }

        private void SetTargetPositionCameraFromWpfCamera(Camera wpfCamera)
        {
            PerspectiveCamera perspectiveCamera;

            perspectiveCamera = wpfCamera as PerspectiveCamera;

            if (perspectiveCamera == null)
                return;


            // prevent calling OnCameraChanged
            _isCameraChangedInternally = true;

            TargetPositionCamera1.CreateFrom(perspectiveCamera);
            
            _isCameraChangedInternally = false;
        }


        private void GetModelCenterAndSize(Model3D model, out Point3D center, out double size)
        {
            Rect3D bounds;

            // Get transformations from root model to this model
            var modelTransform = Ab3d.Utilities.TransformationsHelper.GetModelTotalTransform(ObjectsRoot, model, addFinalModelTransformation: false);

            bounds = model.Bounds;

            if (modelTransform != null)
                bounds = modelTransform.TransformBounds(bounds);

            center = new Point3D(bounds.X + bounds.SizeX / 2,
                                 bounds.Y + bounds.SizeY / 2,
                                 bounds.Z + bounds.SizeZ / 2);

            size = Math.Sqrt(bounds.SizeX * bounds.SizeX +
                             bounds.SizeY * bounds.SizeY +
                             bounds.SizeZ * bounds.SizeZ);
        }

        private static void GetVector3DAnglesInRad(Vector3D vector, out double heading, out double attitude)
        {
            heading = Math.Atan2(vector.X, -vector.Z); // Z must be negated (tested by trial and error)
            attitude = Math.Atan2(vector.Y, Math.Sqrt(vector.X * vector.X + vector.Z * vector.Z));
        }

        private static void GetVector3DAnglesInDegrees(Vector3D vector, out double heading, out double attitude)
        {
            GetVector3DAnglesInRad(vector, out heading, out attitude);

            heading *= 180 / Math.PI;
            attitude *= 180 / Math.PI;
        }


        private void UpateCameraLight()
        {
            if (_isCameraLightShown)
                TargetPositionCamera1.ShowCameraLight = Ab3d.Common.Cameras.ShowCameraLightType.Always;
            else
                TargetPositionCamera1.ShowCameraLight = Ab3d.Common.Cameras.ShowCameraLightType.Never;
        }

        public void ClearSelection()
        {
            RemoveLine();
        }

        private Model3D _selectedModel;

        public void RefreshSelection()
        {
            if (_selectedModel == null)
                return;

            ShowSelectionLines(_selectedModel);
        }

        public void ShowSelectionLines(Model3D selectedModel)
        {
            SelectedModelDecorator.TargetModel3D = selectedModel;
            
            SelectedModelDecorator.ShowBoundingBox = SelectionShowBoundingBox;
            SelectedModelDecorator.ShowNormals = SelectionShowNormals;
            SelectedModelDecorator.ShowTriangles = SelectionShowTriangles;
        }

        public void RemoveLine()
        {
            SelectedModelDecorator.TargetModel3D = null;

            //SelectionsGroup.Children.Clear();

            _selectedModel = null;
        }

        #region Mouse handling

        public Model3D LastHitModel3D { get; private set; }

        public event EventHandler Model3DSelected;
               
        void ObjectsPreviewPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only proceed if in ObjectSelection mode or if CTRL is clicked
            if (_rootModel3D == null)
                return;

            LastHitModel3D = null;

            Point currentMousePosition = e.GetPosition(MainViewport3D);

            var rayResult = VisualTreeHelper.HitTest(this.MainViewport3D, currentMousePosition) as RayMeshGeometry3DHitTestResult;
            
            Model3D selectedModel = rayResult != null ? rayResult.ModelHit : null;


            OnModel3DSelected(selectedModel);


            // Check if CTRL is also pressed
            // In this case we also center the camera to selected object
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (selectedModel == null)
                    SetSceneCamera();
                else
                    SetObjectCamera(selectedModel);
            }
        }

        private HitTestResultBehavior BeginHitTestResultHandler(HitTestResult result)
        {
            RayMeshGeometry3DHitTestResult rayResult;

            rayResult = result as RayMeshGeometry3DHitTestResult;

            OnModel3DSelected(rayResult.ModelHit);

            return HitTestResultBehavior.Stop;
        }

        private void OnModel3DSelected(Model3D selectedModel3D)
        {
            LastHitModel3D = selectedModel3D;

            if (Model3DSelected != null)
                Model3DSelected(this, null);
        }
        #endregion
    }
}
