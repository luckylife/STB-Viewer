using System.Windows;
using System.Windows.Controls;

namespace STBViewer2Lib.DetailsWindow
{
    public partial class DetailsWindow : Window
    {
        private int tabCounter = 1;
        private int _itemCounter = 1;


        public DetailsWindow(List<IPropertyTab> tabs)
        {
            InitializeComponent();
            InitializeDynamicViews(tabs);

            this.Closed += DetailsWindow_Closed;
        }

        // ウィンドウが閉じたときにリソースを解放する処理
        private void DetailsWindow_Closed(object? sender, System.EventArgs e)
        {
            // ここに必要なリソース解放処理を追加
            foreach (UIElement child in MainGrid.Children)
            {
                if (child is StackPanel stackPanel)
                {
                    // 子要素をクリア
                    stackPanel.Children.Clear();
                }
            }
            MainGrid.Children.Clear();
        }

        private void InitializeDynamicViews(List<IPropertyTab> tabs)
        {
            // カラムを動的に追加する場合
            for (int i = 0; i < tabs.Count(); i++) // 
            {
                // 新しいカラムを定義
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());

                StackPanel stackPanel = tabs.ElementAt(i).CreateStackPanel();

                Grid.SetColumn(stackPanel, i); // カラム位置を設定

                // Gridに追加
                _ = MainGrid.Children.Add(stackPanel);
            }
        }
    }


}
