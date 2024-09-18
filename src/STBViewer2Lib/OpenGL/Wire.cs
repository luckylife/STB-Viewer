using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STBViewer2Lib.Shaders;

namespace STBViewer2Lib.OpenGL
{
    public class Wire : IRender
    {
        // 2点の座標
        public Vector3 StartPoint { get; set; }
        public Vector3 EndPoint { get; set; }

        // 線の色
        private Color4 color;

        // 選択状態のフラグ
        public bool isSelected { get; set; }

        // シェーダープログラムの参照
        private ShaderLoader shader;

        // VAOとVBOの参照
        private int vao;
        private int vbo;
        private List<float> vertices;

        public Wire(Vector3 startPoint, Vector3 endPoint, ShaderLoader shader)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            this.shader = shader; // シェーダーを初期化

            // 頂点データを生成
            vertices = [];
            AddVertexData(StartPoint, color);
            AddVertexData(EndPoint, color);

            // VAO、VBOの初期化
            InitializeBuffers();
        }

        private void AddVertexData(Vector3 position, Color4 color)
        {
            vertices.Add(position.X);
            vertices.Add(position.Y);
            vertices.Add(position.Z);
            vertices.Add(color.R);
            vertices.Add(color.G);
            vertices.Add(color.B);
            vertices.Add(color.A);
        }

        private void InitializeBuffers()
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            // VAOをバインド
            GL.BindVertexArray(vao);

            // VBOに頂点データをバインド
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.DynamicDraw);

            // シェーダーのaPosition属性を設定
            int positionLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            // シェーダーのaColor属性を設定
            int colorLocation = GL.GetAttribLocation(shader.Handle, "aColor");
            GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(colorLocation);

            // VAOとVBOのバインドを解除
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> GetBoundingPoints()
        {
            return new Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>>(new Vector3[] { StartPoint, EndPoint }, new Vector3[][] { });
        }

        // 色を変更するメソッド
        public void SetColor(Color4 newColor)
        {
            color = newColor;

            // 新しい色データを設定
            vertices[3] = newColor.R;
            vertices[4] = newColor.G;
            vertices[5] = newColor.B;
            vertices[6] = newColor.A;

            vertices[10] = newColor.R;
            vertices[11] = newColor.G;
            vertices[12] = newColor.B;
            vertices[13] = newColor.A;

            // VBOに色データを更新
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            // 頂点1の色を更新
            float[] vertex1Color = new float[] { vertices[3], vertices[4], vertices[5], vertices[6] };
            GL.BufferSubData(BufferTarget.ArrayBuffer, 3 * sizeof(float), 4 * sizeof(float), vertex1Color);

            // 頂点2の色を更新
            float[] vertex2Color = new float[] { vertices[10], vertices[11], vertices[12], vertices[13] };
            GL.BufferSubData(BufferTarget.ArrayBuffer, (7 + 3) * sizeof(float), 4 * sizeof(float), vertex2Color);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // シェーダー対応の線を描画するメソッド
        public void Render(Matrix4 view, Matrix4 projection)
        {
            shader.Use();

            // ビューと投影行列を渡す
            int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
            int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");

            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(projectionLocation, false, ref projection);

            // モデル行列
            Matrix4 model = Matrix4.Identity;
            int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref model);

            // VAOをバインドして描画
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);  // 線は2つの頂点で描画
            GL.BindVertexArray(0);
        }

        // 後始末 (オブジェクト解放)
        public void Dispose()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
        }
    }
}
