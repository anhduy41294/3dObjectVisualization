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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThreeDObjectViewer
{
    /// <summary>
    /// Interaction logic for Reader3dsSettingsControl.xaml
    /// </summary>
    public partial class Reader3dsSettingsControl : UserControl
    {
        public event EventHandler TransparencySortingChanged;

        public bool ImportSpecularMaterial
        {
            get { return ImportSpecularMaterialCheckbox.IsChecked ?? false; }
        }

        public bool DisableTransparencies
        {
            get { return DisableTransparenciesCheckbox.IsChecked ?? false; }
        }

        public bool ForceTwoSided
        {
            get { return ForceTwoSidedCheckbox.IsChecked ?? false; }
        }

        public bool UseModelTransforms
        {
            get { return UseModelTransformsCheckbox.IsChecked ?? false; }
        }

        public bool ReadBrokenFiles
        {
            get { return ReadBrokenFilesCheckbox.IsChecked ?? false; }
        }

        public Ab3d.Utilities.TransparencySorter.SortingModeTypes TransparencySortingMode
        {
            get
            {
                Ab3d.Utilities.TransparencySorter.SortingModeTypes transparencySorting;

                switch (TransparencySortingComboBox.SelectedIndex)
                {
                    case 0:
                        transparencySorting = Ab3d.Utilities.TransparencySorter.SortingModeTypes.Disabled;
                        break;

                    case 1:
                        transparencySorting = Ab3d.Utilities.TransparencySorter.SortingModeTypes.Simple;
                        break;

                    case 2:
                    default:
                        transparencySorting = Ab3d.Utilities.TransparencySorter.SortingModeTypes.ByCameraDistance;
                        break;
                }

                return transparencySorting;
            }
        }


        public string TexturesPath
        {
            get { return TexuresPathTextBox.Text; }
        }


        public Reader3dsSettingsControl()
        {
            InitializeComponent();


            ShadingInfoControl.InfoText =
@"Shading ComboBox specifies how shading is applied to the read 3D model. The following options are available:

SmoothingGroups
This options uses the SmoothingGroups values stored in 3ds file to define which edges are smooth and which are flat. This options produces the results as in the 3d model designer.

Smooth
All read objects appear smooth - there are no sharp edges - as a sphere.

Flat
All read objects appear flat shaded - all the edges are sharp and not smooth - as a cube.

None
No special shading is applied to the objects. This is the fastest option. Objects usually look smooth but because it is not necessary that all positions are unique, the rendered objects can have some anomalies.";


            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            SetSavedViewOptions();
        }

        public void SetSavedViewOptions()
        {
            ForceTwoSidedCheckbox.IsChecked = Properties.Settings.Default.ForceTwoSidedMaterials;
            ReadBrokenFilesCheckbox.IsChecked = Properties.Settings.Default.TryToReadBrokenFiles;
            UseModelTransformsCheckbox.IsChecked = Properties.Settings.Default.UseModelTransform;
        }

        public void SaveViewOptions()
        {
            Properties.Settings.Default.ForceTwoSidedMaterials = ForceTwoSidedCheckbox.IsChecked ?? false;
            Properties.Settings.Default.TryToReadBrokenFiles = ReadBrokenFilesCheckbox.IsChecked ?? false;
            Properties.Settings.Default.UseModelTransform = UseModelTransformsCheckbox.IsChecked ?? false;

            Properties.Settings.Default.Save();
        }

        private void SelectTexuresPathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFolderDlg = new Microsoft.Win32.OpenFileDialog();
            //openFolderDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            openFolderDlg.Title = "Please choose a textures folder";
            openFolderDlg.CheckFileExists = false;

            openFolderDlg.FileName = "[Get Folder…]";
            openFolderDlg.Filter = "Folders|no.files";
            bool? result = openFolderDlg.ShowDialog();

            if (result ?? false)
            {
                string dir_path = System.IO.Path.GetDirectoryName(openFolderDlg.FileName);
                if (!string.IsNullOrEmpty(dir_path))
                    TexuresPathTextBox.Text = dir_path;
            }
        }


#if READER3D
        public Ab3d.Reader3ds.ShadingType Shading
        {
            get
            {
                string selectedShading;
                Ab3d.Reader3ds.ShadingType shading;

                selectedShading = ShadingComboBox.Text;

                switch (selectedShading)
                {
                    case "None":
                        shading = Ab3d.Reader3ds.ShadingType.None;
                        break;

                    case "Smooth":
                        shading = Ab3d.Reader3ds.ShadingType.Smoooth;
                        break;

                    case "Flat":
                        shading = Ab3d.Reader3ds.ShadingType.Flat;
                        break;

                    case "SmoothingGroups":
                    default:
                        shading = Ab3d.Reader3ds.ShadingType.SmoothingGroups;
                        break;
                }

                return shading;
            }
        }
#endif
    }
}
