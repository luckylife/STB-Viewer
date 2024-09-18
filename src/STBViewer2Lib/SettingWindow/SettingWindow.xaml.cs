using STBViewer2Lib.SettingWindow;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace STBViewer2Lib
{
    public partial class SettingsWindow : Window
    {
        // 設定が保存されたことを通知するイベント
        public event EventHandler<SettingsEventArgs> SettingsSaved;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ViewerSettings _viewerSettings { get; private set; } = new ViewerSettings();

        public SettingsWindow(ViewerSettings settings, IST_BRIDGE stBridge)
        {
            InitializeComponent();
            Update(settings, stBridge);
            DataContext = this; // DataContextを自身に設定してバインディングを有効にする

            // イベントハンドラを動的に設定
            OrthographicRadioButton.Checked += OnProjectionModeChanged;
            PerspectiveRadioButton.Checked += OnProjectionModeChanged;
            this.Closing += SettingWindow_Closing;
        }

        private void SettingWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        // 投影モードの変更処理
        private void OnProjectionModeChanged(object sender, RoutedEventArgs e)
        {
            ComboBox.IsEnabled = PerspectiveRadioButton.IsChecked == false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.isSync = SyncCheckBox.IsChecked == true;
            _viewerSettings.isInvert = InvertCheckBox.IsChecked == true;
            _viewerSettings.SetCameraSetting(PerspectiveRadioButton != null && PerspectiveRadioButton.IsChecked != true, ComboBox.SelectedIndex, FrontBox.Text, DepthBox.Text);

            // 色設定の保存
            _viewerSettings.categorySetting.SelectionColor = ((SolidColorBrush)SelectionColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbNodeColor = ((SolidColorBrush)StbNodeColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbColumnColor = ((SolidColorBrush)StbColumnColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbPostColor = ((SolidColorBrush)StbPostColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbGirderColor = ((SolidColorBrush)StbGirderColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbBeamColor = ((SolidColorBrush)StbBeamColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbBraceColor = ((SolidColorBrush)StbBraceColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbSlabColor = ((SolidColorBrush)StbSlabColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbWallColor = ((SolidColorBrush)StbWallColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbParapetColor = ((SolidColorBrush)StbParapetColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbFoundationColumnColor = ((SolidColorBrush)StbFoundationColumnColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbFootingColor = ((SolidColorBrush)StbFootingColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbStripFootingColor = ((SolidColorBrush)StbStripFootingColorPreview.Fill).Color;
            _viewerSettings.categorySetting.StbPileColor = ((SolidColorBrush)StbPileColorPreview.Fill).Color;

            // 表示/非表示の設定を保存
            _viewerSettings.categorySetting.ShowStbNode = ShowStbNodeCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbColumn = ShowStbColumnCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbPost = ShowStbPostCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbGirder = ShowStbGirderCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbBeam = ShowStbBeamCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbBrace = ShowStbBraceCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbSlab = ShowStbSlabCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbWall = ShowStbWallCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbParapet = ShowStbParapetCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbFoundationColumn = ShowStbFoundationColumnCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbFooting = ShowStbFootingCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbStripFooting = ShowStbStripFootingCheckBox.IsChecked == true;
            _viewerSettings.categorySetting.ShowStbPile = ShowStbPileCheckBox.IsChecked == true;

            // 設定保存イベントを発火
            SettingsSaved?.Invoke(this, new SettingsEventArgs(_viewerSettings));
        }

        // 色を選択するメソッド
        private System.Windows.Media.Color ChooseColor(ref System.Windows.Shapes.Rectangle colorPreview, System.Windows.Media.Color currentColor)
        {
            ColorDialog colorDialog = new()
            {
                Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B)
            };
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                currentColor = System.Windows.Media.Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                colorPreview.Fill = new SolidColorBrush(currentColor);
            }

            return currentColor;
        }

        // 各部材ごとの色選択イベントハンドラ
        private void ChooseSelectionColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.SelectionColor = ChooseColor(ref SelectionColorPreview, _viewerSettings.categorySetting.SelectionColor);
        }

        private void ChooseStbNodeColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbNodeColor = ChooseColor(ref StbNodeColorPreview, _viewerSettings.categorySetting.StbNodeColor);
        }

        private void ChooseStbColumnColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbColumnColor = ChooseColor(ref StbColumnColorPreview, _viewerSettings.categorySetting.StbColumnColor);
        }

        private void ChooseStbPostColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbPostColor = ChooseColor(ref StbPostColorPreview, _viewerSettings.categorySetting.StbPostColor);
        }

        private void ChooseStbGirderColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbGirderColor = ChooseColor(ref StbGirderColorPreview, _viewerSettings.categorySetting.StbGirderColor);
        }

        private void ChooseStbBeamColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbBeamColor = ChooseColor(ref StbBeamColorPreview, _viewerSettings.categorySetting.StbBeamColor);
        }

        private void ChooseStbBraceColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbBraceColor = ChooseColor(ref StbBraceColorPreview, _viewerSettings.categorySetting.StbBraceColor);
        }

        private void ChooseStbSlabColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbSlabColor = ChooseColor(ref StbSlabColorPreview, _viewerSettings.categorySetting.StbSlabColor);
        }

        private void ChooseStbWallColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbWallColor = ChooseColor(ref StbWallColorPreview, _viewerSettings.categorySetting.StbWallColor);
        }

        private void ChooseStbParapetColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbParapetColor = ChooseColor(ref StbParapetColorPreview, _viewerSettings.categorySetting.StbParapetColor);
        }

        private void ChooseStbFoundationColumnColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbFoundationColumnColor = ChooseColor(ref StbFoundationColumnColorPreview, _viewerSettings.categorySetting.StbFoundationColumnColor);
        }

        private void ChooseStbFootingColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbFootingColor = ChooseColor(ref StbFootingColorPreview, _viewerSettings.categorySetting.StbFootingColor);
        }

        private void ChooseStbStripFootingColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbStripFootingColor = ChooseColor(ref StbStripFootingColorPreview, _viewerSettings.categorySetting.StbStripFootingColor);
        }

        private void ChooseStbPileColor_Click(object sender, RoutedEventArgs e)
        {
            _viewerSettings.categorySetting.StbPileColor = ChooseColor(ref StbPileColorPreview, _viewerSettings.categorySetting.StbPileColor);
        }

        public ViewerSettings GetUpdatedSettings()
        {
            return _viewerSettings;
        }

        // ST-Bridgeファイル読み込み後、階層メニューを作成するメソッド
        public void Update(ViewerSettings settings, IST_BRIDGE stBridge)
        {
            _viewerSettings = settings.Clone();
            _viewerSettings.SetST_Bridge(stBridge);

            SyncCheckBox.IsChecked = _viewerSettings.isSync;
            InvertCheckBox.IsChecked = _viewerSettings.isInvert;
            if (_viewerSettings.CameraSetting().IsOrtho)
            {
                OrthographicRadioButton.IsChecked = true;
            }
            else
            {
                PerspectiveRadioButton.IsChecked = true;
            }

            ComboBox.Items.Clear();
            foreach (string item in _viewerSettings.floorGrids)
            {
                ComboBox.Items.Add(item);
            }
            ComboBox.SelectedIndex = settings.index;
            FrontBox.Text = _viewerSettings.CameraSetting().orthoSetting.Near.ToString();
            DepthBox.Text = _viewerSettings.CameraSetting().orthoSetting.Far.ToString();


            // 色設定を反映
            SelectionColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.SelectionColor);
            StbNodeColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbNodeColor);
            StbColumnColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbColumnColor);
            StbPostColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbPostColor);
            StbGirderColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbGirderColor);
            StbBeamColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbBeamColor);
            StbBraceColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbBraceColor);
            StbSlabColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbSlabColor);
            StbWallColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbWallColor);
            StbParapetColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbParapetColor);
            StbFoundationColumnColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbFoundationColumnColor);
            StbFootingColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbFootingColor);
            StbStripFootingColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbStripFootingColor);
            StbPileColorPreview.Fill = new SolidColorBrush(_viewerSettings.categorySetting.StbPileColor);

#if !DEBUG
            // 色設定グループを非表示にして、UIを詰める
            ColorSettingsGroup.Visibility = Visibility.Collapsed;
#endif

            // 表示/非表示のチェックボックスに現在の設定を反映
            ShowStbNodeCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbNode;
            ShowStbColumnCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbColumn;
            ShowStbPostCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbPost;
            ShowStbGirderCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbGirder;
            ShowStbBeamCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbBeam;
            ShowStbBraceCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbBrace;
            ShowStbSlabCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbSlab;
            ShowStbWallCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbWall;
            ShowStbParapetCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbParapet;
            ShowStbFoundationColumnCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbFoundationColumn;
            ShowStbFootingCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbFooting;
            ShowStbStripFootingCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbStripFooting;
            ShowStbPileCheckBox.IsChecked = _viewerSettings.categorySetting.ShowStbPile;
        }

        public void DeleteOrthoViews()
        {
            this.ComboBox.Items.Clear();
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // 入力が数値であるかどうかを確認
            e.Handled = !IsTextNumeric(e.Text);
        }

        // 入力文字列が数値かどうかを確認するヘルパーメソッド
        private bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out _); // 数値として解釈できるかを確認
        }

    }

    // 設定を渡すためのイベント引数
    public class SettingsEventArgs : EventArgs
    {
        public ViewerSettings Settings { get; }

        public SettingsEventArgs(ViewerSettings settings)
        {
            Settings = settings;
        }
    }
}

