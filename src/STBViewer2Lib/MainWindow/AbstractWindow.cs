using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;

namespace STBViewer2Lib.MainWindow
{
    public abstract class AbstractWindow : Window
    {
        // エラーメッセージを保持するリスト
        protected List<string> validationErrors = [];

        protected AbstractTreeView3DSet? _leftTreeView3DSet;
        protected AbstractTreeView3DSet? _rightTreeView3DSet; // ST-BridgeB用
        protected ViewerSettings _viewerSettings = new();
        protected SettingsWindow? _settingsWindow;

        protected abstract string GetSchemaPath();
        protected abstract string GetVersion();

        // 右側のTreeView3DSetを追加
        protected abstract void InitializeRightTreeView3DSet();

        // ST-Bridgeファイルの読み込み処理
        protected abstract IST_BRIDGE? LoadSTBridgeFile(string filePath, Encoding encoding);

        protected abstract void ClearSTBridgeA_Click(object sender, RoutedEventArgs e);

        protected abstract void ClearSTBridgeB_Click(object sender, RoutedEventArgs e);

        public void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OpenSettingsWindow_Click(sender, e);
            }
        }

        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 破棄しない。Hide()で非表示にするだけ
            _settingsWindow = new SettingsWindow(_viewerSettings, null)
            {
                Owner = this
            };

            // 設定保存イベントを監視
            _settingsWindow.SettingsSaved += SettingsWindow_SettingsSaved;

        }

        // 設定ウィンドウをモードレスで表示
        public void OpenSettingsWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow.Visibility != Visibility.Visible)
            {
                _settingsWindow.Show();
            }
            else
            {
                // ウィンドウが最小化されている場合は復元
                if (_settingsWindow.WindowState == WindowState.Minimized)
                {
                    _settingsWindow.WindowState = WindowState.Normal;
                }

                // ウィンドウを最前面に持ってくる
                _ = _settingsWindow.Activate();
            }
        }

        // 表示の初期化（ビューのリセット）
        public void ResetView_Click(object sender, RoutedEventArgs e)
        {
            _leftTreeView3DSet.ResetCameraView(); // 左側のビューをリセット
            _rightTreeView3DSet?.ResetCameraView(); // 右側のビューをリセット
        }

        // バージョン情報の表示
        public void ShowVersion_Click(object sender, RoutedEventArgs e)
        {
            _ = System.Windows.MessageBox.Show("ST-Bridge Viewer バージョン 1.0", "バージョン情報", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // // 妥当性検証のコールバック
        public void ValidationCallback(object sender, ValidationEventArgs e)
        {
            // エラーメッセージに行番号と列番号を追加する
            string locationInfo = "";

            if (e.Exception is XmlSchemaException schemaException)
            {
                locationInfo = $" (Line: {schemaException.LineNumber}, Position: {schemaException.LinePosition})";
            }

            // 警告かエラーかでメッセージを分岐
            if (e.Severity == XmlSeverityType.Warning)
            {
                validationErrors.Add($"Warning: {e.Message}{locationInfo}");
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                validationErrors.Add($"Error: {e.Message}{locationInfo}");
            }
        }

        public void SettingsWindow_SettingsSaved(object sender, SettingsEventArgs e)
        {
            // 保存された設定を取得して反映する
            _viewerSettings = e.Settings;
            CameraSetting setting = _viewerSettings.CameraSetting();
            _leftTreeView3DSet.ApplySetting(_viewerSettings.categorySetting, setting);
            _rightTreeView3DSet?.ApplySetting(_viewerSettings.categorySetting, setting);
        }



        // ST-BridgeAの読み込み
        public virtual void LoadSTBridgeA_Click(object sender, RoutedEventArgs e)
        {
            // ファイル選択ダイアログ
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Filter = "ST-Bridge Files (*.stb)|*.stb",
                Title = "ST-BridgeAのファイルを選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                (string encoding, string version) = CheckEncoding(openFileDialog.FileName);
                if (version != GetVersion())
                {
                    _ = System.Windows.MessageBox.Show("バージョンが一致しません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _leftTreeView3DSet.ClearModel(); // 左側をクリア

                // エラーメッセージリストを初期化
                validationErrors.Clear();

                string filePath = openFileDialog.FileName;
                try
                {
                    Encoding encode = Encoding.GetEncoding(encoding);
                    IST_BRIDGE stbData = LoadSTBridgeFile(filePath, encode); // ST-Bridgeファイルを読み込む処理
                    try
                    {
                        // ModelManagerを使用して3Dモデルを読み込み
                        _leftTreeView3DSet.LoadModelFromSTBridge(stbData, _viewerSettings.categorySetting, _viewerSettings.CameraSetting(), false);

                        _settingsWindow.Update(_viewerSettings, stbData);

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

        // リソースファイルからスキーマを読み込む
        public static string GetEmbeddedXsd(Assembly assembly, string resourcePath, Encoding encoding)
        {
            using Stream stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                throw new FileNotFoundException("リソースが見つかりません: " + resourcePath);
            }

            using StreamReader reader = new(stream, encoding);
            return reader.ReadToEnd(); // XSDファイルの内容を文字列として返す
        }

        public static (string, string) CheckEncoding(string filePath)
        {
            // デフォルトのエンコーディング
            string encoding = string.Empty;
            string version = string.Empty;
            // まずは、ファイルをバイナリで読み込み、BOM (Byte Order Mark) を確認する
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new(fileStream, detectEncodingFromByteOrderMarks: true);
            // ファイルの最初の部分を読んで、エンコーディングを検出
            char[] buffer = new char[1024];
            int readChars = reader.Read(buffer, 0, buffer.Length);

            // 読み込んだ文字列を検査
            string xmlSnippet = new(buffer, 0, readChars);

            // XMLの宣言部分をパース
            using (StringReader stringReader = new(xmlSnippet))
            {
                using XmlReader xmlReader = XmlReader.Create(stringReader);
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        version = xmlReader.GetAttribute("version");
                        continue;
                    }

                    // 開始要素をチェック（最初の "ST_BRIDGE" などの要素）
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.HasAttributes)
                    {
                        // 属性をループして "version" 属性をチェック
                        while (xmlReader.MoveToNextAttribute())
                        {
                            if (xmlReader.Name == "version")
                            {
                                version = xmlReader.Value; // versionの値を取得
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(version))
                        {
                            break;
                        }

                        // 属性を元の位置に戻す
                        _ = xmlReader.MoveToElement();
                    }

                }
            }

            // BOMや宣言がない場合は、StreamReaderの検出したエンコーディングを使用
            if (string.IsNullOrEmpty(encoding))
            {
                encoding = reader.CurrentEncoding.WebName;
            }

            return (encoding, version);
        }
    }
}