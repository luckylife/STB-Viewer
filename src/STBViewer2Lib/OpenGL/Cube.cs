using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STBViewer2Lib.Shaders;

namespace STBViewer2Lib.OpenGL
{
    public class Cube : IRender
    {
        // 立方体の底面の中心座標
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        // 立方体のサイズ
        public float Size { get; set; }

        // 立方体の回転角度（度単位）
        public float Rotation { get; set; }

        // 立方体の色
        private Color4 color;

        // 選択状態のフラグ
        public bool isSelected { get; set; }

        private ShaderLoader shader;  // シェーダーオブジェクト
        private int vao;
        private int vbo;
        private int ebo;
        private float[] vertices;
        private uint[] indices;

        public Cube(float centerX, float centerY, float centerZ, float size, float rotation, ShaderLoader shader)
        {
            X = centerX;
            Y = centerY;
            Z = centerZ;
            Size = size;
            Rotation = rotation;
            this.shader = shader;
            InitializeCube();
        }

        private void InitializeCube()
        {
            // 立方体の頂点データと初期色を設定
            vertices = GenerateCubeVertices(Size);
            indices = GenerateCubeIndices();

            // VAO, VBO, EBOの生成とバインド
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // 頂点属性設定 (位置)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // 頂点属性設定 (色)
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        private float[] GenerateCubeVertices(float size)
        {
            float halfSize = size / 2.0f;
            List<float> verticesList = [];

            // 回転行列を生成（Z軸周りの回転）
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation));

            // 頂点座標を生成して回転を適用
            Vector3[] positions = {
        // 前面
        new(-halfSize, -halfSize,  halfSize),
        new( halfSize, -halfSize,  halfSize),
        new( halfSize,  halfSize,  halfSize),
        new(-halfSize,  halfSize,  halfSize),

        // 背面
        new(-halfSize, -halfSize, -halfSize),
        new( halfSize, -halfSize, -halfSize),
        new( halfSize,  halfSize, -halfSize),
        new(-halfSize,  halfSize, -halfSize),
    };

            // 各頂点に回転を適用し、カラー情報を追加（初期色は白）
            foreach (Vector3 pos in positions)
            {
                // 回転行列を適用して新しい座標を計算
                Vector3 rotatedPos = Vector3.TransformPosition(pos, rotationMatrix);
                verticesList.AddRange(new float[] { rotatedPos.X, rotatedPos.Y, rotatedPos.Z, 1.0f, 1.0f, 1.0f, 1.0f });
            }

            return verticesList.ToArray();
        }

        private uint[] GenerateCubeIndices()
        {
            return new uint[]
            {
                // 前面
                0, 1, 2,
                2, 3, 0,

                // 背面
                4, 5, 6,
                6, 7, 4,

                // 左側面
                0, 3, 7,
                7, 4, 0,

                // 右側面
                1, 2, 6,
                6, 5, 1,

                // 上面
                3, 2, 6,
                6, 7, 3,

                // 底面
                0, 1, 5,
                5, 4, 0,
            };
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

        // シェーダーを使用して立方体を描画するメソッド
        public void Render(Matrix4 view, Matrix4 projection)
        {
            // ビューとプロジェクション行列をシェーダーに渡す
            int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
            int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");

            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(projectionLocation, false, ref projection);

            // モデル行列（平行移動のみ）
            Matrix4 model = Matrix4.CreateTranslation(X, Y, Z);
            int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref model);

            // VAOをバインドして描画
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        public Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> GetBoundingPoints()
        {
            return new Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>>([new Vector3(X, Y, Z)], []);
        }

        public void Dispose()
        {
            // VAOとVBOを解放
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}
