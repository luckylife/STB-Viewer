using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STBViewer2Lib.DetailsWindow;
using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;
using STBViewer2Lib.Shaders;
using System.Diagnostics;
using System.Windows;

namespace STBViewer2Lib.MainWindow
{
    public abstract class AbstractModelManager
    {
        public const float ScaleFactor = 0.001f;  // mm単位からm単位へのスケーリング

        protected GLControl _glControl;
        protected List<IModelElement> modelElements; // IModelElementのリストで管理
        protected CameraSetting cameraSetting;
        protected System.Drawing.Point lastMousePosition;
        protected CategorySetting _categorySettings;

        protected bool isLeftButtonDown = false;
        protected bool isRightButtonDown = false;
        protected ShaderLoader shader;

        public AbstractModelManager(GLControl glControl)
        {
            modelElements = [];
            _glControl = glControl;
            _glControl.Load += GlControl_Load;
            _glControl.Resize += glControl_Resize;
            _glControl.Paint += (sender, e) => RenderModel();
            _glControl.MouseClick += GlControl_MouseClick;

            // マウスイベントの登録
            _glControl.MouseMove += glControl_MouseMove;
            _glControl.MouseWheel += glControl_MouseWheel;
            _glControl.MouseDown += glControl_MouseDown;
            _glControl.MouseUp += glControl_MouseUp;
            InitilizeCamera();
        }

        public abstract void LoadModelFromSTBridge(IST_BRIDGE istbData, CategorySetting categorySetting, CameraSetting cameraSetting, bool isModelB);

        private void InitilizeCamera()
        {
            cameraSetting = new CameraSetting();
        }

        // 現在のカメラ設定を取得
        public CameraSetting GetSetting()
        {
            return cameraSetting.Clone();
        }

        // 指定されたカメラ設定を適用
        public void ApplyCameraSetting(CameraSetting setting)
        {
            cameraSetting = setting.Clone();
            _glControl.Invalidate();  // 再描画
        }

        public void ClearModel()
        {
            modelElements.Clear();
            _glControl.Invalidate();  // 3Dビューの再描画をトリガー
        }

        public void ResetCameraView()
        {
            AdjustCameraForParallelProjection();
            _glControl.Invalidate();  // 3Dビューの再描画をトリガー
        }

        // GLControlの初期化
        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1f); // 画面のクリア色を黒に設定
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest); // 奥行きテストを有効化
            GL.DepthFunc(DepthFunction.Less); // 深度テストの方式
            shader = new("vertex_shader.glsl", "fragment_shader.glsl");
        }

        protected void AdjustCameraForParallelProjection()
        {
            cameraSetting.SetDefault();
            Resize();
        }

        private void Resize()
        {
            _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
        }

        // ウィンドウリサイズ時のビューポート設定
        private void glControl_Resize(object sender, EventArgs e)
        {
            Resize();
        }

        // マウスクリックイベントハンドラ
        private void GlControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _glControl.MakeCurrent(); // OpenGLコンテキストをアクティブにする

            // 右クリック時は選択された部材の詳細を表示
            if (e.Button == MouseButtons.Right)
            {
                ShowSelectedElementDetails();
                return;
            }

            // 左クリック時は部材の選択
            if (e.Button == MouseButtons.Left)
            {
                IModelElement? selectedElement = null;
                Vector2 mousePosition = new(e.X, e.Y);

                // ここでビューポートを適切に取得し、クリック位置を考慮する
                int[] viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);

                // カメラの設定を適用
                SetupCamera(out Matrix4 view, out Matrix4 projection);

                // TODO: 要調整　Z値の小さすぎるものは選択しない
                float zNear = 0.00001f;
                float minZ = 0.99999f;
                foreach (IModelElement element in modelElements)
                {
                    if (element.isEnable == false)
                    {
                        continue;
                    }

                    List<Vector2?> tempBoundingPoints = element.GetBoundingPoints().Item1.Select(point => ProjectToScreen(point, view, projection, viewport)).ToList();

                    List<Vector2> boundingPoints = [];
                    foreach (Vector2? point in tempBoundingPoints)
                    {
                        if (point == null)
                        {
                            continue;
                        }
                        boundingPoints.Add(point.Value);
                    }

                    List<Vector3> outer = element.GetBoundingPoints().Item1.ToList();
                    if (outer.Count >= 3)
                    {
                        if (IsPointInPolygon(mousePosition, outer, viewport))
                        {
                            // Z値を比較して、最も手前の要素を選択
                            float z = GetZValue(mousePosition, view, projection, viewport, element);
                            if (zNear < z && z < minZ)
                            {
                                minZ = z;
                                selectedElement = element;
                            }
                        }
                    }
                    else
                    {

                        if (boundingPoints.Count == 2)
                        {
                            // 線分として距離を計算
                            Vector2 start = boundingPoints[0];
                            Vector2 end = boundingPoints[1];
                            float distance = DistanceFromPointToLineSegment(mousePosition, start, end);

                            if (distance < 10f)
                            {
                                // Z値を比較して、最も手前の要素を選択
                                float z = GetZValue(mousePosition, view, projection, viewport, element);
                                if (zNear < z && z < minZ)
                                {
                                    minZ = z;
                                    selectedElement = element;
                                }
                            }
                        }
                        else if (boundingPoints.Count == 1)
                        {
                            float distance = (boundingPoints[0] - mousePosition).Length;

                            if (distance < 10f)
                            {
                                // Z値を比較して、最も手前の要素を選択
                                float z = GetZValue(mousePosition, view, projection, viewport, element);
                                if (zNear < z && z < minZ)
                                {
                                    minZ = z;
                                    selectedElement = element;
                                }
                            }
                        }

                    }
                }

                // 選択状態の更新
                foreach (IModelElement element in modelElements)
                {
                    element.isSelected = element == selectedElement;
                    element.SetColor(element.isSelected ? Color.Red : Color.White);
                }

                _glControl.Invalidate();
            }
        }

        // Zバッファからクリック位置のZ値を取得するヘルパーメソッド
        private float GetZValue(Vector2 mousePosition, Matrix4 view, Matrix4 projection, int[] viewport, IModelElement element)
        {
            // 要素の中心座標を取得
            Vector3 elementCenter = element.GetBoundingPoints().Item1.First();
            Vector2? screenPos = ProjectToScreen(elementCenter, view, projection, viewport);

            if (screenPos.HasValue)
            {
                float x = screenPos.Value.X;
                float y = screenPos.Value.Y;

                // Zバッファの値を取得（クリック位置のZ値を読み取る）
                float[] zValue = new float[1];
                GL.ReadPixels((int)x, (int)y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, zValue);
                GL.ReadPixels((int)x, (int)y, 1, 1, PixelFormat.DepthComponent, PixelType.Float, zValue);
                return zValue[0];
            }
            return float.MaxValue;
        }

        private Vector3 GetElementCenter(IModelElement element)
        {
            IEnumerable<Vector3> boundingPoints = element.GetBoundingPoints().Item1;
            Vector3 center = new();
            foreach (Vector3 point in boundingPoints)
            {
                center += point;
            }
            center /= boundingPoints.Count();
            return center;
        }
        // 選択された部材の詳細を表示するメソッド
        private void ShowSelectedElementDetails()
        {
            IModelElement? selectedElement = modelElements.FirstOrDefault(element => element.isSelected);
            if (selectedElement != null)
            {
                // 詳細情報を生成（プロパティ名と値のリスト）
                List<IPropertyTab> tabs = selectedElement.GetDetails();

                // 詳細ウィンドウをモーダルで表示
                STBViewer2Lib.DetailsWindow.DetailsWindow detailsWindow = new(tabs);
                _ = detailsWindow.ShowDialog(); // モーダルで開く
            }
            else
            {
                _ = System.Windows.Forms.MessageBox.Show("部材が選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 点と線分の最短距離を計算するメソッド
        private float DistanceFromPointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            Vector2 pointToStart = point - lineStart;

            float t = Vector2.Dot(pointToStart, line) / Vector2.Dot(line, line);
            t = Math.Clamp(t, 0.0f, 1.0f);

            Vector2 projection = lineStart + (t * line);

            return (point - projection).Length;
        }

        // 3D座標をスクリーン座標に変換するメソッド
        private Vector2? ProjectToScreen(Vector3 position, Matrix4 modelViewMatrix, Matrix4 projectionMatrix, int[] viewport)
        {
            // 位置ベクトルを拡張して、4次元のベクトルを作成 (w=1)
            Vector4 clipSpacePos = new(position, 1.0f);

            // 変換を手動で行う (modelViewMatrix と projectionMatrix を掛け算する)
            clipSpacePos = Vector4.TransformRow(clipSpacePos, modelViewMatrix * projectionMatrix);

            if (clipSpacePos.W == 0.0f)
            {
                return null;
            }

            Vector3 ndcSpacePos = new Vector3(clipSpacePos.X, clipSpacePos.Y, clipSpacePos.Z) / clipSpacePos.W;

            float screenX = ((ndcSpacePos.X + 1.0f) / 2.0f * viewport[2]) + viewport[0];
            float screenY = ((1.0f - ndcSpacePos.Y) / 2.0f * viewport[3]) + viewport[1];

            return new Vector2(screenX, screenY);
        }

        public void RenderModel()
        {
            // OpenGLコンテキストをアクティブにする
            _glControl.MakeCurrent();
            Resize();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // カメラの設定を適用
            SetupCamera(out Matrix4 view, out Matrix4 projection);

            // シェーダーを使用してビューと投影行列を適用
            shader.Use(); // シェーダーをアクティブに
            int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
            int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");
            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(projectionLocation, false, ref projection);


            foreach (IModelElement element in modelElements)
            {
                if (element.isSelected)
                {
                    element.SetColor(Color4.Red); // 選択時の色
                }
                else
                {
                    element.SetElementColor(_categorySettings); // 通常時の色
                }
                element.Render(view, projection);
            }

            // シェーダーの使用を終了
            GL.UseProgram(0);

            GL.Flush();
            _glControl.SwapBuffers(); // バッファをスワップして描画を反映
        }

        private void SetupCamera(out Matrix4 view, out Matrix4 projection)
        {
            if (cameraSetting.IsOrtho)
            {
                // 平行投影モードの場合のカメラ設定
                OrthoSetting ortho = cameraSetting.orthoSetting;

                // カメラの方向ベクトルを使ってカメラ位置を計算
                Vector3 cameraPosition = cameraSetting.CameraTarget - (cameraSetting.CameraDirection * ortho.Near * ScaleFactor);

                // ビュー行列を設定（カメラの位置とターゲットの位置を基にする）
                view = Matrix4.LookAt(cameraPosition, cameraSetting.CameraTarget, ortho.UpDirection);

                // CameraDirectionによって、最大幅・最大高さを算定し、その最大値と
                Tuple<float, float> max = cameraSetting.GetMax();
                float width = max.Item1;
                float height = max.Item2;

                // ウィンドウのアスペクト比を取得
                float windowAspect = (float)_glControl.Width / _glControl.Height;
                float orthoWidth, orthoHeight;
                // 幅と高さのどちらに基準を合わせるか決めて投影行列を作成
                if (windowAspect > 1.0f)  // 横長の場合
                {
                    orthoWidth = width / ortho.Ratio;
                    orthoHeight = orthoWidth / windowAspect;
                }
                else  // 縦長の場合
                {
                    orthoHeight = height / ortho.Ratio;
                    orthoWidth = orthoHeight * windowAspect;
                }

                // NearClip と FarClip の自動算定
                // カメラとターゲットの距離を計算
                float distanceToTarget = (cameraPosition - cameraSetting.CameraTarget).Length;

                // NearClipとFarClipの設定（ユーザーの操作に合わせてスケール）
                float nearClip = distanceToTarget - (ortho.Near * ScaleFactor);  // カメラターゲットから近い方のクリップ距離
                float farClip = distanceToTarget + (ortho.Far * ScaleFactor);    // カメラターゲットから遠い方のクリップ距離

                // NearClip と FarClip の制約を設定（範囲内で安全にクリップ）
                nearClip = Math.Max(nearClip, 0.01f);  // NearClipは負数にならないように最小値を設定

                // 平行投影行列を作成
                projection = Matrix4.CreateOrthographic(orthoWidth, orthoHeight, nearClip, farClip);
            }
            else
            {
                // 透視投影モードの場合のカメラ設定
                PerspectiveSetting perspective = cameraSetting.perspectiveSetting;

                // カメラ方向ベクトルの回転行列を生成 (カメラ方向を基に回転行列を計算)
                Vector3 forward = cameraSetting.CameraDirection.Normalized();
                Vector3 right = Vector3.Cross(Vector3.UnitZ, forward).Normalized();  // CAD系なのでZ軸との外積で右方向ベクトルを算定
                Vector3 up = Vector3.Cross(forward, right);  // 上方向ベクトルを再計算

                // カメラの位置を更新（ターゲットからカメラ距離分だけ離れた位置）
                Vector3 cameraPosition = cameraSetting.CameraTarget - (cameraSetting.CameraDirection * perspective.CameraDistance);

                // ビュー行列を設定（カメラの位置とターゲットの位置を基にする）
                view = Matrix4.LookAt(cameraPosition, cameraSetting.CameraTarget, up);

                // ウィンドウのアスペクト比を取得
                float aspectRatio = _glControl.Width / (float)_glControl.Height;

                // 透視投影行列を作成
                projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), aspectRatio, perspective.NearClip, perspective.FarClip);
            }
        }

        // マウスホイールでズーム
        private void glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (cameraSetting.IsOrtho)
            {
                cameraSetting.orthoSetting.Ratio += e.Delta > 0 ? 0.1f : -0.1f;
            }
            else
            {
                cameraSetting.perspectiveSetting.CameraDistance *= e.Delta > 0 ? 0.9f : 1.1f;
            }
            _glControl.Invalidate();
        }

        // マウス移動で回転
        private void glControl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _glControl.MakeCurrent();
                if (cameraSetting.IsOrtho)
                {
                    // 平行投影モードでは回転を無効化
                }
                else
                {
                    float deltaX = e.X - lastMousePosition.X;
                    float deltaY = e.Y - lastMousePosition.Y;

                    float sensitivity = 0.005f; // 回転の感度

                    // カメラの現在の向きベクトルを取得
                    Vector3 currentDirection = cameraSetting.CameraDirection;

                    // マウスのX移動に基づくY軸周りの回転（左右の回転）
                    Matrix4 yawRotation = Matrix4.CreateFromAxisAngle(Vector3.UnitZ, -deltaX * sensitivity);
                    currentDirection = Vector3.TransformNormal(currentDirection, yawRotation);

                    // マウスのY移動に基づくX軸周りの回転（上下の回転）
                    // 上下方向への回転を制限するために、X軸方向の回転を制御
                    Vector3 right = Vector3.Cross(currentDirection, Vector3.UnitZ); // 現在のカメラ方向に垂直な右方向ベクトルを計算
                    Matrix4 pitchRotation = Matrix4.CreateFromAxisAngle(right, -deltaY * sensitivity);
                    currentDirection = Vector3.TransformNormal(currentDirection, pitchRotation);

                    // 新しい方向ベクトルを正規化して保存
                    cameraSetting.CameraDirection = currentDirection.Normalized();
                }
                _glControl.Invalidate(); // 再描画をトリガー
            }
            else if (e.Button == MouseButtons.Middle || (isLeftButtonDown && isRightButtonDown))
            {
                _glControl.MakeCurrent();
                // 中ボタンを押しているか、もしくは左クリックと右クリックを同時に押しているかチェック
                Debug.WriteLine("Middle button pressed and moving.");
                float deltaX = e.X - lastMousePosition.X;
                float deltaY = e.Y - lastMousePosition.Y;

                float panSpeed = 0.01f;
                if (cameraSetting.IsOrtho)
                {
                    if (cameraSetting.CameraDirection == new Vector3(0, 0, -1))
                    {
                        cameraSetting.CameraTarget += new Vector3(-deltaX * panSpeed, deltaY * panSpeed, 0);
                    }
                    else
                    {

                        cameraSetting.CameraTarget += new Vector3(-deltaX * panSpeed * cameraSetting.CameraDirection.Y, deltaX * panSpeed * cameraSetting.CameraDirection.X, deltaY * panSpeed);
                    }
                }
                else
                {
                    cameraSetting.CameraTarget += new Vector3(-deltaX * panSpeed, deltaY * panSpeed, 0);
                }
                _glControl.Invalidate(); // 再描画をトリガー
            }

            lastMousePosition = e.Location;
        }

        private void glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftButtonDown = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                isRightButtonDown = true;
            }
        }

        private void glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftButtonDown = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                isRightButtonDown = false;
            }
        }

        private void TreeViewStructure_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is CustomTreeNode selectedTreeNode)
            {
                if (selectedTreeNode is IModelElement selectedModel)
                {
                    // 3Dモデル側の選択状態を更新
                    foreach (IModelElement element in modelElements)
                    {
                        bool isSelected = element == selectedModel;
                        element.isSelected = isSelected;
                    }

                    _glControl.Invalidate(); // 再描画をトリガー
                }
            }
        }

        private bool IsPointInPolygon(Vector2 point, List<Vector3> polygon, int[] viewport)
        {
            // Vector2? ではなく Vector2 をリストとして扱う
            List<Vector2> projectedPolygon = [];

            // カメラの設定を適用
            SetupCamera(out Matrix4 view, out Matrix4 projection);

            // 3D座標を2Dスクリーン座標に変換
            foreach (Vector3 vertex in polygon)
            {
                Vector2? projectedPoint = ProjectToScreen(vertex, view, projection, viewport);
                if (projectedPoint != null)
                {
                    projectedPolygon.Add(projectedPoint.Value);
                }
            }

            // プロジェクション結果が空の場合は処理を終了
            if (projectedPolygon.Count == 0)
            {
                return false;
            }

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (Vector2? projectedPoint in projectedPolygon)
            {
                if (projectedPoint == null)
                {
                    continue;
                }
                if (projectedPoint.Value.X < minX)
                {
                    minX = projectedPoint.Value.X;
                }
                if (projectedPoint.Value.X > maxX)
                {
                    maxX = projectedPoint.Value.X;
                }
                if (projectedPoint.Value.Y < minY)
                {
                    minY = projectedPoint.Value.Y;
                }
                if (projectedPoint.Value.Y > maxY)
                {
                    maxY = projectedPoint.Value.Y;
                }
            }

            return point.X >= minX && point.X <= maxX &&
                   point.Y >= minY && point.Y <= maxY;
        }

        public void ApplySetting(CategorySetting categorySetting, CameraSetting settings)
        {
            _categorySettings = categorySetting;
            cameraSetting = settings;
            foreach (IModelElement element in modelElements)
            {
                element.SetElementEnable(categorySetting);
            }
            _glControl.Invalidate();
        }
    }
}