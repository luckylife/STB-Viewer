using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STBViewer2Lib.Shaders;

namespace STBViewer2Lib.OpenGL
{

    public class Sphere : IRender
    {
        // 球の座標
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // 球の半径
        public float Radius { get; set; }
        public bool isSelected { get; set; }

        // 球の色
        private Color4 color;
        private ShaderLoader shader;
        private int vao, vbo;
        private float[] vertices;

        public Sphere(float x, float y, float z, float radius, ShaderLoader shader)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            this.shader = shader;

            // VAO、VBOを生成
            InitializeBuffers();
        }

        public Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> GetBoundingPoints()
        {
            return new Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>>([new Vector3(X, Y, Z)], []);
        }

        // 色を変更するメソッド
        public void SetColor(Color4 newColor)
        {
            color = newColor;

            // 色データを更新
            for (int i = 0; i < vertices.Length / 7; i++)
            {
                vertices[(i * 7) + 3] = color.R;
                vertices[(i * 7) + 4] = color.G;
                vertices[(i * 7) + 5] = color.B;
                vertices[(i * 7) + 6] = color.A;
            }

            // VBOに新しい色データをアップロード
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // シェーダー対応のRenderメソッド
        public void Render(Matrix4 view, Matrix4 projection)
        {
            // ビュー行列と投影行列をシェーダーに渡す
            int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
            int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");

            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(projectionLocation, false, ref projection);

            // モデル行列（球体の位置）
            Matrix4 model = Matrix4.CreateTranslation(X, Y, Z);
            int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref model);

            // 球体を描画
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, vertices.Length / 7);
            GL.BindVertexArray(0);
        }

        private void InitializeBuffers()
        {
            // 球体の頂点データを生成
            vertices = GenerateSphereVertices(Radius, 16, 16);

            // VAOとVBOの生成とバインド
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // 頂点属性設定 (位置)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // 頂点属性設定 (色)
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        private float[] GenerateSphereVertices(float radius, int slices, int stacks)
        {
            List<float> verticesList = [];

            for (int i = 0; i <= slices; i++)
            {
                double theta1 = i * 2 * Math.PI / slices;
                _ = (i + 1) * 2 * Math.PI / slices;

                for (int j = 0; j <= stacks; j++)
                {
                    double phi = j * Math.PI / stacks;
                    double sinPhi = Math.Sin(phi);
                    double cosPhi = Math.Cos(phi);

                    // 頂点座標
                    float x1 = radius * (float)(Math.Cos(theta1) * sinPhi);
                    float y1 = radius * (float)(Math.Sin(theta1) * sinPhi);
                    float z1 = radius * (float)cosPhi;

                    // 頂点色（初期は白色）
                    verticesList.AddRange(new float[] { x1, y1, z1, color.R, color.G, color.B, color.A });
                }
            }

            return verticesList.ToArray();
        }

        // リソースを解放するメソッド
        public void Dispose()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }
    }
}
