using OpenTK.GLControl;
using ST_BRIDGE201;
using STBViewer2Lib;
using STBViewer2Lib.MainWindow;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace STBViewer2_201.MainWindow
{
    public partial class MainWindow_201 : AbstractWindow
    {
        public MainWindow_201()
        {
            InitializeComponent();
            GLControl leftgLControl = new();
            _leftTreeView3DSet = new TreeView3DSet_201(LeftPanel, leftgLControl);  // 左のパネル
            GLControl rightglControl = new();
            _rightTreeView3DSet = new TreeView3DSet_201(RightPanel, rightglControl); // 右のパネル

            _leftTreeView3DSet.CameraSettingChanged += setting =>
            {
                if (_viewerSettings.isSync)
                {
                    _rightTreeView3DSet.ApplyCameraSetting(setting);
                }
            };

            _rightTreeView3DSet.CameraSettingChanged += setting =>
            {
                if (_viewerSettings.isSync)
                {
                    _leftTreeView3DSet.ApplyCameraSetting(setting);
                }
            };

            // PreviewKeyDownイベントの登録
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            Loaded += OnLoaded;
        }

        protected override void InitializeRightTreeView3DSet()
        {
            GLControl gLControl = new();
            _rightTreeView3DSet = new TreeView3DSet_201(RightPanel, gLControl);
            RightPanel.Visibility = Visibility.Visible;
        }

        protected override string GetVersion()
        {
            return "2.0.1";
        }

        public override void LoadSTBridgeA_Click(object sender, RoutedEventArgs e)
        {
            base.LoadSTBridgeA_Click(sender, e);

            // ST-BridgeAが読み込まれたら、ST-BridgeBのメニュー項目を有効化
            MenuItem_LoadSTBridgeB.IsEnabled = true;
            MenuItem_ClearSTBridgeB.IsEnabled = true;
        }

        // ST-BridgeAのクリア
        protected override void ClearSTBridgeA_Click(object sender, RoutedEventArgs e)
        {
            _leftTreeView3DSet.ClearModel(); // 左側をクリア
            _rightTreeView3DSet?.ClearModel(); // 右側をクリア

            // 右側のパネルを非表示にして、列定義を1列に戻す
            RightPanel.Visibility = Visibility.Collapsed;
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ST-BridgeBのメニュー項目を再度無効化
            MenuItem_LoadSTBridgeB.IsEnabled = false;
            MenuItem_ClearSTBridgeB.IsEnabled = false;

            _viewerSettings.DeleteOrthoViews();
            _settingsWindow.Update(_viewerSettings, null);
        }

        // ST-BridgeBの読み込み
        protected void LoadSTBridgeB_Click(object sender, RoutedEventArgs e)
        {
            // ファイル選択ダイアログ
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Filter = "ST-Bridge Files (*.stb)|*.stb",
                Title = "ST-BridgeBのファイルを選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                (string encoding, string version) = CheckEncoding(openFileDialog.FileName);
                if (version != GetVersion())
                {
                    _ = System.Windows.MessageBox.Show("バージョンが一致しません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Gridの列定義を2分割に変更
                MainGrid.ColumnDefinitions.Clear();
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // 右側のパネルを表示
                RightPanel.Visibility = Visibility.Visible;

                if (_rightTreeView3DSet == null)
                {
                    //InitializeRightTreeView3DSet(); // 右側のパネルが初期化されていなければ初期化
                }

                _rightTreeView3DSet.ClearModel(); // 右側をクリア

                // エラーメッセージリストを初期化
                validationErrors.Clear();

                string filePath = openFileDialog.FileName;
                try
                {
                    Encoding encode = Encoding.GetEncoding(encoding);
                    IST_BRIDGE stbData = LoadSTBridgeFile(filePath, encode); // ST-Bridgeファイルを読み込む処理
                    try
                    {
                        _rightTreeView3DSet.LoadModelFromSTBridge(stbData, _viewerSettings.categorySetting, _leftTreeView3DSet.GetCameraSetting(), true); // 右側に読み込む
                    }
                    catch (Exception ex)
                    {
                        _ = System.Windows.MessageBox.Show($"モデルの読み込みに失敗しました: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _ = System.Windows.MessageBox.Show($"エラーが発生しました: {ex.Message}");
                }
            }
        }

        // ST-BridgeBのクリア
        protected override void ClearSTBridgeB_Click(object sender, RoutedEventArgs e)
        {
            // 右側のパネルを非表示にして、列定義を1列に戻す
            RightPanel.Visibility = Visibility.Collapsed;
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _rightTreeView3DSet?.ClearModel(); // 右側をクリア
        }

        // スキーマのパス
        protected override string GetSchemaPath()
        {
            return "STBViewer2_201.Resources.STBridge_v201.xsd";
        }

        protected override IST_BRIDGE LoadSTBridgeFile(string filePath, Encoding encoding)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // スキーマファイルをリソースから取得
            string schemaContent = GetEmbeddedXsd(assembly, GetSchemaPath(), encoding);

            // XMLリーダー設定（妥当性検証用）
            XmlReaderSettings settings = new()
            {
                ValidationType = ValidationType.Schema
            };
            _ = settings.Schemas.Add(null, XmlReader.Create(new StringReader(schemaContent)));
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

            // 妥当性検証をしながらファイルを読み込む
            using XmlReader reader = XmlReader.Create(filePath, settings);
            XmlSerializer serializer = new(typeof(ST_BRIDGE)); // STBridgeクラスに合わせる
            ST_BRIDGE? stbData = (ST_BRIDGE)serializer.Deserialize(reader);

            // 妥当性チェック後にメッセージを表示する
            if (validationErrors.Count > 0)
            {
                string allErrors = string.Join("\n", validationErrors);
                _ = System.Windows.MessageBox.Show($"ST-BridgeAファイルを読み込みましたが、以下の警告/エラーがありました:\n\n{allErrors}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return stbData;
        }

    }
}
