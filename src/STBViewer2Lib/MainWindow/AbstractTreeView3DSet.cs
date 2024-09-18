using OpenTK.GLControl;
using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;

namespace STBViewer2Lib.MainWindow
{
    public abstract class AbstractTreeView3DSet
    {
        public event Action<CameraSetting>? CameraSettingChanged; // カメラ設定が変更されたときに発火
        protected readonly System.Windows.Controls.TreeView _treeView;
        protected AbstractModelManager _modelManager;
        protected List<IModelElement> _modelElements = [];
        protected readonly ObservableCollection<CustomTreeNode> _treeNodes = [];
        private bool isLeftMouseButtonDown = false;
        private bool isRightMouseButtonDown = false;

        public AbstractTreeView3DSet(Grid parentPanel, GLControl _glControl)
        {
            // TreeViewの作成
            _treeView = new System.Windows.Controls.TreeView
            {
                ItemsSource = _treeNodes,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // 仮想化を有効にするために添付プロパティを設定
            _treeView.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, true);
            _treeView.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
            _treeView.SetValue(ScrollViewer.IsDeferredScrollingEnabledProperty, true);

            // HierarchicalDataTemplateをコードで作成して設定
            HierarchicalDataTemplate template = new(typeof(CustomTreeNode));

            // 子ノードをバインド
            System.Windows.Data.Binding itemsBinding = new()
            {
                Path = new PropertyPath("Children")
            };

            template.ItemsSource = itemsBinding;
            // 各ノードの表示内容（TextBlockとして表示）
            FrameworkElementFactory textBlockFactory = new(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Name"));
            template.VisualTree = textBlockFactory;

            // TreeViewにテンプレートを適用
            _treeView.ItemTemplate = template;

            _treeView.SelectedItemChanged += TreeViewStructure_SelectedItemChanged;
            _treeView.PreviewMouseRightButtonDown += TreeView_PreviewMouseRightButtonDown;

            // GLControlの作成
            WindowsFormsHost glHost = new()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                // GLControlのインスタンスを作成
                Child = _glControl
            };

            _glControl.MouseMove += (sender, e) =>
            {
                if (isLeftMouseButtonDown || isRightMouseButtonDown)
                {
                    OnCameraSettingChanged();
                }
            };
            _glControl.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isLeftMouseButtonDown = true;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    isRightMouseButtonDown = true;
                }
            };
            _glControl.MouseUp += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isLeftMouseButtonDown = false;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    isRightMouseButtonDown = false;
                }
            };

            _glControl.MouseWheel += (sender, e) => OnCameraSettingChanged();


            // レイアウト: TreeViewとGLControlを左右に並べる
            Grid grid = new();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            // TreeViewを左側のカラムに配置
            Grid.SetColumn(_treeView, 0);
            _ = grid.Children.Add(_treeView);

            // GLControlを右側のカラムに配置
            Grid.SetColumn(glHost, 1);
            _ = grid.Children.Add(glHost);


            // ParentPanelにGridを追加
            parentPanel.Children.Clear();
            _ = parentPanel.Children.Add(grid);

        }

        // カメラ設定が変更されたときに発火させるメソッド
        private void OnCameraSettingChanged()
        {
            CameraSettingChanged?.Invoke(_modelManager.GetSetting());
        }

        // ST-Bridgeモデルの読み込み
        public void LoadModelFromSTBridge(IST_BRIDGE stbData, CategorySetting categorySetting, CameraSetting cameraSetting, bool isModelB)
        {
            // モデルを読み込み、TreeViewにバインドする
            _modelElements.Clear();
            _treeNodes.Clear();
            CustomTreeNode? rootNode = CustomTreeNode.ConvertToTreeNode(stbData, _modelElements, "ST_BRIDGE");
            _treeNodes.Add(rootNode);
            _treeView.ItemsSource = _treeNodes;
            _modelManager.LoadModelFromSTBridge(stbData, categorySetting, cameraSetting, isModelB);
        }

        // モデルのクリア
        public void ClearModel()
        {
            _modelElements.Clear();
            _treeView.ItemsSource = null;
            _modelManager.ClearModel();
        }

        // カメラビューのリセット
        public void ResetCameraView()
        {
            _modelManager.ResetCameraView();
        }

        // TreeViewの選択項目が変更されたときの処理
        private void TreeViewStructure_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // TreeViewの選択が変更されたときの動作をここに記述
            if (e.NewValue is CustomTreeNode selectedNode && selectedNode.ModelElement != null)
            {
                // すべてのモデル要素の選択を解除
                foreach (IModelElement element in _modelElements)
                {
                    element.isSelected = false;
                }

                // TreeViewで選択された要素を3Dモデルで選択
                selectedNode.ModelElement.isSelected = true;

                // 3Dモデルの再描画を要求
                _modelManager.RenderModel();
            }
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_treeView.SelectedItem is CustomTreeNode selectedNode)
            {
                _ = System.Windows.MessageBox.Show(selectedNode.Details, "Node Details");
            }
        }

        // ユーティリティ: クリックされたTreeViewの項目を取得
        private TreeViewItem? GetClickedTreeViewItem(MouseButtonEventArgs e)
        {
            DependencyObject? dependencyObject = (DependencyObject)e.OriginalSource;
            while (dependencyObject is not null and not TreeViewItem)
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            return dependencyObject as TreeViewItem;
        }

        // 現在のカメラ設定を取得
        public CameraSetting GetCameraSetting()
        {
            return _modelManager.GetSetting();
        }

        // カメラ設定を適用
        public void ApplyCameraSetting(CameraSetting setting)
        {
            _modelManager.ApplyCameraSetting(setting);
        }

        public void ApplySetting(CategorySetting categorySetting, CameraSetting cameraSetting)
        {
            _modelManager.ApplySetting(categorySetting, cameraSetting);
        }
    }
}
