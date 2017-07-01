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
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace ThreeDObjectViewer
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        private string _selectedObjectName;
        public string SelectedObjectName
        {
            get
            {
                return _selectedObjectName;
            }
            set
            {
                _selectedObjectName = value;

                if (!string.IsNullOrEmpty(_selectedObjectName))
                    this.Title = _selectedObjectName + " info";
                else
                    this.Title = "Info";
            }
        }

        public Model3D SelectedModel { get; set; }

        public Dictionary<string, object> NamedObjects { get; set; }
        public Dictionary<object, string> ObjectNames { get; set; }

        //public Ab3d.Reader3ds UsedReader3ds { get; set; }

        public InfoWindow()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(InfoWindow_Loaded);
        }

        void InfoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedModel == null)
                this.Close();

            if (SelectedModel is Model3DGroup)
                StatisticsTabItem.Header = "Model3DGroup info";
            else
                StatisticsTabItem.Header = "GeometryModel3D info";

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            if (!string.IsNullOrEmpty(StatisticsTextBox.Text))
                return;

            if (SelectedModel == null)
                return;

            StatisticsTextBox.Text = Ab3d.Utilities.Dumper.GetModelInfoString(SelectedModel, NamedObjects);
        }

        private void UpdateHierarchy()
        {
            if (!string.IsNullOrEmpty(HierarchyTextBox.Text) || !(SelectedModel is Model3DGroup))
                return;

            var hierarchyLogText = new StringBuilder();
            DumpWpfHierarchy((Model3DGroup)SelectedModel, 0, hierarchyLogText, " ", Environment.NewLine);

            HierarchyTextBox.Text = hierarchyLogText.ToString();
        }

        private void UpdateDump()
        {
            if (!string.IsNullOrEmpty(DumpTextBox.Text))
                return;

        	if (SelectedModel is GeometryModel3D)
            {
                int maxLineCount;

                if (LimitDumpCheckBox.IsChecked ?? false)
                    maxLineCount = 300;
                else
                    maxLineCount = -1; // unlimited

                DumpTextBox.Text = Ab3d.Utilities.Dumper.GetDumpString(((GeometryModel3D)SelectedModel).Geometry as MeshGeometry3D, maxLineCount, "0.00");
            }
            else
            {
                DumpTextBox.Text = "Dump mesh is only available when a GeometryModel3D is selected.";
            }
        }

        private void LimitDumpCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            DumpTextBox.Text = "";
            UpdateDump();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            if (TabControl1.SelectedIndex == 0)
                UpdateStatistics();
            else if (TabControl1.SelectedIndex == 1)
                UpdateDump();
            else if (TabControl1.SelectedIndex == 2)
                UpdateHierarchy();
        }

        private void DumpWpfHierarchy(Model3DGroup rootModelGroup, int indent, StringBuilder hierarchyLogText, string spaceText, string newLineText)
        {
            string currentObjectName;
            string currentModelGroupText;
            string modelTypeDescription;

            for (int i = 0; i < (indent * 4); i++)
                hierarchyLogText.Append(spaceText);

            if (!ObjectNames.TryGetValue(rootModelGroup, out currentObjectName))
                currentObjectName = "";

            currentModelGroupText = string.Format("\"{0}\" as Model3DGroup{1}", currentObjectName, newLineText);

            hierarchyLogText.Append(currentModelGroupText);

            foreach (Model3D oneModel in rootModelGroup.Children)
            {
                if (oneModel is Model3DGroup)
                    DumpWpfHierarchy((oneModel as Model3DGroup), indent + 1, hierarchyLogText, spaceText, newLineText);
                else
                {
                    for (int i = 0; i < ((indent + 1) * 4); i++)
                        hierarchyLogText.Append(spaceText);


                    if (!ObjectNames.TryGetValue(oneModel, out currentObjectName))
                        currentObjectName = "";

                    modelTypeDescription = oneModel.GetType().Name;

                    hierarchyLogText.Append(string.Format("\"{0}\" as {1}{2}", currentObjectName, modelTypeDescription, newLineText));
                }
            }

            //if (addNodeIds)
            //    hierarchyLogText.Append(string.Format(" ({0}, {1})", rootMeshObject.NodeId, rootMeshObject.ParentNodeId));

            //hierarchyLogText.Append(newLineText);

            //foreach (MeshObject oneMeshObject in rootMeshObject.Children)
            //    DumpOriginalHierarchy(oneMeshObject, indent + 2, hierarchyLogText, spaceText, newLineText, addNodeIds);
        }
    }
}
