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
using System.Reflection;

namespace ThreeDObjectViewer
{
    /// <summary>
    /// Interaction logic for ElementsTreeViewPanel.xaml
    /// </summary>
    public partial class ElementsTreeViewPanel : UserControl
    {
        private Dictionary<object, string> _objectNames;
        private Model3D _rootModel;

        private object _selectedObject;

        private Dictionary<TreeViewItem, object> _treeViewElements;
        private Dictionary<object, TreeViewItem> _treeViewObjects;

        private List<TreeViewItem> _parentItemsToRecreate;

        public delegate void SelectedElementChangedEventHandler(object sender, object selectedObject);
        public event SelectedElementChangedEventHandler SelectedElementChanged;

        public delegate void CameraCenterChangedEventHandler(object sender, object selectedObject);
        public event CameraCenterChangedEventHandler CameraCenterChanged;

        public event EventHandler InfoButtonClicked;

        public ElementsTreeViewPanel()
        {
            InitializeComponent();
        }

        private void RefreshTreeView()
        {
            if (_rootModel == null || _objectNames == null)
                return;

            // TODO: Presever opened and selected elements
            object savedSelectedObject = _selectedObject;
            if (_selectedObject != null)
                ClearSelection();

            FillTreeView(_rootModel as Model3DGroup, _objectNames);

            if (savedSelectedObject != null)
                SelectTreeViewItem(savedSelectedObject);    
        }

        public void ApplyTransparencySorting()
        {
            TreeViewItem treeViewItem;

            if (_treeViewObjects == null || _parentItemsToRecreate == null)
                return;


            foreach (TreeViewItem oneParentItem in _parentItemsToRecreate)
            {
                Model3DGroup parentModelGroup = _treeViewElements[oneParentItem] as Model3DGroup;

                if (parentModelGroup == null)
                    continue;

                oneParentItem.BeginInit();
                oneParentItem.Items.Clear();

                foreach (Model3D model3D in parentModelGroup.Children)
                {
                    treeViewItem = _treeViewObjects[model3D];

                    if (treeViewItem.Parent == null)
                        oneParentItem.Items.Add(treeViewItem);
                }

                oneParentItem.EndInit();
            }
        }

        public void ChangeObjectPosition(Model3D model)
        {
            TreeViewItem treeViewItem;

            if (_treeViewObjects == null || model == null)
                return;
            
            if (!_treeViewObjects.TryGetValue(model, out treeViewItem))
                return; // not found

            TreeViewItem parentItem = treeViewItem.Parent as TreeViewItem;

            if (parentItem != null)
            {
                if (_parentItemsToRecreate == null)
                    _parentItemsToRecreate = new List<TreeViewItem>();

                // If the parent TreeViewItem was not yet set to be recreated, add it to the _parentItemsToRecreate
                if (!_parentItemsToRecreate.Contains(parentItem))
                    _parentItemsToRecreate.Add(parentItem);
            }
        }

        public void ClearTreeView()
        {
            ElementsTreeView.Items.Clear();

            _rootModel = null;
            _objectNames = null;
            _treeViewElements = null;
            _treeViewObjects = null;
            _parentItemsToRecreate = null;

            InfoButton.IsEnabled = false;
        }

        public void FillTreeView(Model3D rootModel, Dictionary<object, string> objectNames)
        {
            TreeViewItem rootItem;

            _rootModel = rootModel;
            _objectNames = objectNames;

            _treeViewElements = new Dictionary<TreeViewItem, object>();
            _treeViewObjects = new Dictionary<object, TreeViewItem>();

            ElementsTreeView.BeginInit();
            ElementsTreeView.Items.Clear();

            if (rootModel != null)
            {
                rootItem = new TreeViewItem();
                rootItem.Header = GetObjectName(rootModel);
                ElementsTreeView.Items.Add(rootItem);

                _treeViewElements.Add(rootItem, rootModel);
                _treeViewObjects.Add(rootModel, rootItem);

                if (rootModel is Model3DGroup)
                    FillTreeView(rootItem, rootModel);

                rootItem.IsExpanded = true;

                InfoButton.IsEnabled = true;
            }

            ElementsTreeView.EndInit();

            NoFileTextBlock.Visibility = Visibility.Collapsed;
            ButtonsStackPanel.Visibility = Visibility.Visible;
            TreeViewScrollViewer.Visibility = Visibility.Visible;

            _parentItemsToRecreate = null;
        }

        public bool SelectTreeViewItem(object selectedModel)
        {
            bool isSelectedModelFound;
            TreeViewItem selectedItem;

            if (selectedModel == null)
                ClearSelection();

            if (_treeViewObjects.TryGetValue(selectedModel, out selectedItem))
            {
                selectedItem.IsSelected = true;
                selectedItem.BringIntoView();

                isSelectedModelFound = true;
            }
            else
            {
                ClearSelection();

                isSelectedModelFound = false;
            }

            return isSelectedModelFound;
        }

        public void ClearSelection()
        {
            if (ElementsTreeView.SelectedItem == null) return;

            (ElementsTreeView.SelectedItem as TreeViewItem).IsSelected = false;

            ClearSelectionButton.IsEnabled = false;

            MoveObjectsPanel.IsEnabled = false;
        }
        
        private void FillTreeView(TreeViewItem currentItem, Model3D currentModel)
        {
            Model3DGroup currentModel3DGroup = currentModel as Model3DGroup;

            if (currentModel3DGroup == null)
            {
                AddTreeViewItem(currentItem, currentModel);
            }
            else
            {
                foreach (Model3D oneModel in currentModel3DGroup.Children)
                    AddTreeViewItem(currentItem, oneModel);
            }
        }

        private void AddTreeViewItem(TreeViewItem currentItem, Model3D oneModel)
        {
            TreeViewItem newItem;

            newItem = new TreeViewItem();
            newItem.Header = GetObjectName(oneModel);
            currentItem.Items.Add(newItem);

            _treeViewElements.Add(newItem, oneModel);
            _treeViewObjects[oneModel] = newItem;

            if (oneModel is Model3DGroup)
                FillTreeView(newItem, oneModel as Model3DGroup);
        }

        private string GetObjectName(object obj)
        {
            string name;

            if (!_objectNames.TryGetValue(obj, out name))
                name = String.Format("[{0}]", obj.GetType().Name);


            return name;
        }

        void TreeViewItemSelected(object sender, RoutedPropertyChangedEventArgs<object> args)
        {
            if (args.NewValue != null)
            {
                object selectedElement;

                selectedElement = _treeViewElements[args.NewValue as TreeViewItem];
                OnSelectedElementChanged(selectedElement);
            }
            else
            {
                OnSelectedElementChanged(null); // selection removed
            }
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSelection();
            OnSelectedElementChanged(null); // selection removed
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem;

            selectedTreeViewItem = ElementsTreeView.SelectedItem as TreeViewItem;

            if (selectedTreeViewItem == null) return;

            object selectedElement = _treeViewElements[selectedTreeViewItem];

            if (CameraCenterChanged != null)
                CameraCenterChanged(this, selectedElement);
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (InfoButtonClicked != null)
                InfoButtonClicked(this, null);
        }

        protected void OnSelectedElementChanged(object selectedObject)
        {
            if (selectedObject == null)
            {
                ClearSelectionButton.IsEnabled = false;
                CenterButton.IsEnabled = false;

                if (_rootModel == null)
                    InfoButton.IsEnabled = false;
                else
                    InfoButton.IsEnabled = true;

                MoveObjectsPanel.IsEnabled = false;
            }
            else
            {
                ClearSelectionButton.IsEnabled = true;
                CenterButton.IsEnabled = true;
                InfoButton.IsEnabled = true;

                MoveObjectsPanel.IsEnabled = true;
            }

            _selectedObject = selectedObject;

            if (SelectedElementChanged != null)
                SelectedElementChanged(this, selectedObject);
        }
        
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedObject == null)
                return;

            Model3D selectedModel = (Model3D)_selectedObject;

            Model3DGroup group = GetModelsParentGroup(selectedModel);

            if (group != null)
            {
                int currentIndex = group.Children.IndexOf(selectedModel);

                if (currentIndex == group.Children.Count - 2)
                {
                    // Pre-last
                    group.Children.RemoveAt(currentIndex);
                    group.Children.Add(selectedModel);

                    RefreshTreeView();
                }
                else if (currentIndex < group.Children.Count - 2)
                {
                    group.Children.RemoveAt(currentIndex);
                    group.Children.Insert(currentIndex + 1, selectedModel);

                    RefreshTreeView();
                }
                // else - on last element - nothing to do
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedObject == null)
                return;

            Model3D selectedModel = (Model3D)_selectedObject;

            Model3DGroup group = GetModelsParentGroup(selectedModel);

            if (group != null)
            {
                int currentIndex = group.Children.IndexOf(selectedModel);

                if (currentIndex > 0)
                {
                    group.Children.RemoveAt(currentIndex);
                    group.Children.Insert(currentIndex - 1, selectedModel);

                    RefreshTreeView();
                }
            }
        }

        private void MoveToLastButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedObject == null)
                return;

            Model3D selectedModel = (Model3D)_selectedObject;

            Model3DGroup group = GetModelsParentGroup(selectedModel);

            if (group != null)
            {
                group.Children.Remove(selectedModel);
                group.Children.Add(selectedModel);

                RefreshTreeView();
            }
        }

        private Model3DGroup GetModelsParentGroup(Model3D model)
        {
            TreeViewItem selectedItem;
            Model3DGroup group;

            if (!_treeViewObjects.TryGetValue(model, out selectedItem))
                return null;

            TreeViewItem parentItem = selectedItem.Parent as TreeViewItem;

            if (parentItem == null)
                return null;

            object parentModelObject;
            _treeViewElements.TryGetValue(parentItem, out parentModelObject);

            if (parentModelObject is Model3DGroup)
                group = (Model3DGroup)parentModelObject;
            else
                group = null;

            return group;
        }
    }
}
