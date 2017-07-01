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

namespace ThreeDObjectViewer
{
    /// <summary>
    /// Interaction logic for OptionsPanel.xaml
    /// </summary>
    public partial class OptionsPanel : UserControl
    {
        // true when there is a file opened, false before any file is loaded
        private bool _isFileOpened;

        public ObjectsPreviewPanel CurrentObjectsPreviewPanel { get; set; }

        public Rect3D CurrentObjectBounds { get; set; }

        public OptionsPanel()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(OptionsPanel_Loaded);
        }

        void OptionsPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentObjectsPreviewPanel == null)
            {
                DisabledSelectionOptionsTextBlock.Visibility = System.Windows.Visibility.Visible;


                ShowBoundsBoxCheckbox.IsChecked = false;
                ShowTrianglesBoxCheckbox.IsChecked = false;
                ShowNormalsBoxCheckbox.IsChecked = false;

                ShowCameraAxisCheckbox.IsChecked = false;
                ShowCameraPreviewCheckbox.IsChecked = false;
                ShowGridCheckbox.IsChecked = false;
                ShowBigAxisCheckbox.IsChecked = false;

                ViewOptionsPanel.IsEnabled = false;
                OptionsStackPanel.IsEnabled = false;
            }
            else
            {
                ShowBoundsBoxCheckbox.IsChecked    = CurrentObjectsPreviewPanel.SelectionShowBoundingBox;
                ShowTrianglesBoxCheckbox.IsChecked = CurrentObjectsPreviewPanel.SelectionShowTriangles;
                ShowNormalsBoxCheckbox.IsChecked   = CurrentObjectsPreviewPanel.SelectionShowNormals;

                ShowCameraAxisCheckbox.IsChecked    = CurrentObjectsPreviewPanel.IsCameraAxisShown;
                ShowCameraPreviewCheckbox.IsChecked = CurrentObjectsPreviewPanel.IsCameraPreviewShown;
                ShowGridCheckbox.IsChecked          = CurrentObjectsPreviewPanel.IsWireGridShown;
                ShowBigAxisCheckbox.IsChecked       = CurrentObjectsPreviewPanel.IsBigAxisShown;
            }

            SetSavedViewOptions();
        }

        public void ResetOptionsForNewOpenedFile()
        {
            _isFileOpened = true;

            UpdateViewOptions();
        }

        private void OnSelectionOptionsChanged(object sender, RoutedEventArgs e)
        {
            UpdateSelectionOptions();
        }

        private void UpdateSelectionOptions()
        {
            if (CurrentObjectsPreviewPanel == null)
                return;

            CurrentObjectsPreviewPanel.SelectionShowBoundingBox = ShowBoundsBoxCheckbox.IsChecked ?? false;
            CurrentObjectsPreviewPanel.SelectionShowTriangles   = ShowTrianglesBoxCheckbox.IsChecked ?? false;
            CurrentObjectsPreviewPanel.SelectionShowNormals     = ShowNormalsBoxCheckbox.IsChecked ?? false;

            CurrentObjectsPreviewPanel.RefreshSelection();

        }

        private void OnViewOptionsChanged(object sender, RoutedEventArgs e)
        {
            UpdateViewOptions();
        }

        public void UpdateViewOptions()
        {
            if (CurrentObjectsPreviewPanel != null)
            {
                CurrentObjectsPreviewPanel.IsCameraAxisShown = ShowCameraAxisCheckbox.IsChecked ?? false;
                CurrentObjectsPreviewPanel.IsCameraPreviewShown = ShowCameraPreviewCheckbox.IsChecked ?? false;

                if (ShowGridCheckbox.IsChecked ?? false)
                {
                    double axisSize;
                    
                    CurrentObjectsPreviewPanel.IsWireGridShown = true;
                    CurrentObjectsPreviewPanel.ShowWireGrid();
                    

                    if (Size10WireGridRadioButton.IsChecked ?? false)
                        CurrentObjectsPreviewPanel.WireGrid.Size = new Size(10, 10);
                    else if (Size100WireGridRadioButton.IsChecked ?? false)
                        CurrentObjectsPreviewPanel.WireGrid.Size = new Size(100, 100);
                    else if (Size1000WireGridRadioButton.IsChecked ?? false)
                        CurrentObjectsPreviewPanel.WireGrid.Size = new Size(1000, 1000);
                    else if (SizeCustomWireGridRadioButton.IsChecked ?? false)
                    {
                        double gridSize;
                        
                        gridSize = Math.Max(CurrentObjectBounds.SizeX, CurrentObjectBounds.SizeZ) * 1.4;

                        if (gridSize > 0)
                            CurrentObjectsPreviewPanel.WireGrid.Size = new Size(gridSize, gridSize);
                        else
                            CurrentObjectsPreviewPanel.WireGrid.Size = new Size(100, 100);
                    }

                    if (ShowBigAxisCheckbox.IsChecked ?? false)
                    {
                        axisSize = (CurrentObjectsPreviewPanel.WireGrid.Size.Width + CurrentObjectsPreviewPanel.WireGrid.Size.Height) / 4 * 1.2;
                        CurrentObjectsPreviewPanel.IsBigAxisShown = true;
                        CurrentObjectsPreviewPanel.ShowBigAxis();
                        CurrentObjectsPreviewPanel.BigAxis.Length = axisSize;
                    }
                    else
                    {
                        CurrentObjectsPreviewPanel.IsBigAxisShown = false;
                        CurrentObjectsPreviewPanel.HideBigAxis();
                    }
                }
                else
                {
                    CurrentObjectsPreviewPanel.IsWireGridShown = false;
                    CurrentObjectsPreviewPanel.IsBigAxisShown = false;

                    CurrentObjectsPreviewPanel.HideWireGrid();
                    CurrentObjectsPreviewPanel.HideBigAxis();
                }
            }
        }

        public void SetSavedViewOptions()
        {
            if (CurrentObjectsPreviewPanel != null)
                return;

            ShowCameraAxisCheckbox.IsChecked    = Properties.Settings.Default.IsCameraAxisShown;
            ShowCameraPreviewCheckbox.IsChecked = Properties.Settings.Default.IsCameraPreviewShown;
            ShowBigAxisCheckbox.IsChecked       = Properties.Settings.Default.IsGridAxisShown;

            string wireGridSettings = Properties.Settings.Default.WireGridSetting;

            if (string.IsNullOrEmpty(wireGridSettings))
            {
                ShowGridCheckbox.IsChecked = false;
            }
            else
            {
                ShowGridCheckbox.IsChecked = true;

                switch (wireGridSettings)
                {
                    case "10":
                        Size10WireGridRadioButton.IsChecked = true;
                        break;

                    case "100":
                        Size100WireGridRadioButton.IsChecked = true;
                        break;

                    case "1000":
                        Size1000WireGridRadioButton.IsChecked = true;
                        break;

                    default:
                        SizeCustomWireGridRadioButton.IsChecked = true;
                        break;
                }
            }
        }

        public void SaveViewOptions()
        {
            Properties.Settings.Default.IsCameraAxisShown    = CurrentObjectsPreviewPanel.IsCameraAxisShown;
            Properties.Settings.Default.IsCameraPreviewShown = CurrentObjectsPreviewPanel.IsCameraPreviewShown;

            if (CurrentObjectsPreviewPanel.BigAxis == null)
                Properties.Settings.Default.IsGridAxisShown = ShowBigAxisCheckbox.IsChecked ?? false;
            else
                Properties.Settings.Default.IsGridAxisShown = CurrentObjectsPreviewPanel.BigAxis.IsVisible;

            string wireGridSettings;

            if (ShowGridCheckbox.IsChecked ?? false)
            {
                if (Size10WireGridRadioButton.IsChecked ?? false)
                    wireGridSettings = "10";
                else if (Size100WireGridRadioButton.IsChecked ?? false)
                    wireGridSettings = "100";
                else if (Size1000WireGridRadioButton.IsChecked ?? false)
                    wireGridSettings = "1000";
                else
                    wireGridSettings = "custom";
            }
            else
            {
                wireGridSettings = ""; // not shown
            }

            Properties.Settings.Default.WireGridSetting = wireGridSettings;

            Properties.Settings.Default.Save();
        }

        private void ShowReader3dsSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
