using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using STBViewer2Lib.Shaders;

namespace STBViewer2Lib.OpenGL
{
    public class Plane : IRender
    {
        // メイン平面の外部ポリゴン
        private readonly List<Vector3> outerVertices;

        // 穴のポリゴンリスト
        private readonly List<List<Vector3>> holeVertices;

        // 色
        private Color4 color;

        public bool isSelected { get; set; }

        private ShaderLoader shader;
        private float[] vertices;
        private int vao;
        private int vbo;

        public Plane(List<Vector3> outerVertices, List<List<Vector3>> holeVertices, ShaderLoader shader)
        {
            this.outerVertices = outerVertices;
            this.holeVertices = holeVertices;
            this.shader = shader;
            InitializePlane();
        }

        // BoundingPointsを返すメソッドを拡張
        public Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> GetBoundingPoints()
        {
            return new Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>>(outerVertices, holeVertices);
        }

        // 平面を初期化するメソッド
        private void InitializePlane()
        {
            List<float> verticesList = [];

            // 外側の頂点データ（位置 + 色）
            foreach (Vector3 vertex in outerVertices)
            {
                verticesList.Add(vertex.X);
                verticesList.Add(vertex.Y);
                verticesList.Add(vertex.Z);
                // 色データ
                verticesList.Add(color.R);
                verticesList.Add(color.G);
                verticesList.Add(color.B);
                verticesList.Add(color.A);
            }

            // 穴の頂点データ（位置 + 色）
            foreach (List<Vector3> hole in holeVertices)
            {
                foreach (Vector3 vertex in hole)
                {
                    verticesList.Add(vertex.X);
                    verticesList.Add(vertex.Y);
                    verticesList.Add(vertex.Z);
                    // 色データ
                    verticesList.Add(color.R);
                    verticesList.Add(color.G);
                    verticesList.Add(color.B);
                    verticesList.Add(color.A);
                }
            }

            vertices = verticesList.ToArray();

            // VAOとVBOの設定
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

        public void SetColor(Color4 newColor)
        {
            color = newColor;

            // 全ての頂点に対して色データを更新
            int totalVertexCount = outerVertices.Count + holeVertices.Sum(hole => hole.Count);
            for (int i = 0; i < totalVertexCount; i++)
            {
                int baseIndex = i * 7;
                vertices[baseIndex + 3] = color.R;
                vertices[baseIndex + 4] = color.G;
                vertices[baseIndex + 5] = color.B;
                vertices[baseIndex + 6] = color.A;
            }

            // VBOに新しい色データをアップロード
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            // ビューとプロジェクション行列をシェーダーに渡す
            int viewLocation = GL.GetUniformLocation(shader.Handle, "view");
            int projectionLocation = GL.GetUniformLocation(shader.Handle, "projection");

            GL.UniformMatrix4(viewLocation, false, ref view);
            GL.UniformMatrix4(projectionLocation, false, ref projection);

            // モデル行列
            Matrix4 model = Matrix4.Identity;
            int modelLocation = GL.GetUniformLocation(shader.Handle, "model");
            GL.UniformMatrix4(modelLocation, false, ref model);

            // ステンシルバッファを有効化
            GL.Enable(EnableCap.StencilTest);
            GL.ClearStencil(0);  // ステンシルバッファの初期化
            GL.Clear(ClearBufferMask.StencilBufferBit);  // ステンシルバッファをクリア

            // 1. 外側ポリゴンをステンシルバッファに書き込む（ステンシル値 = 1）
            GL.ColorMask(false, false, false, false);  // カラーバッファへの書き込みを無効化
            GL.DepthMask(false);  // 深度バッファへの書き込みも無効化

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);  // ステンシル値を1に設定
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            // 外側の平面を描画
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, outerVertices.Count);

            // 2. 穴をステンシルバッファに書き込む（ステンシル値 = 0）
            GL.StencilFunc(StencilFunction.Always, 0, 0xFF);  // 穴の部分を0に設定
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            int holeOffset = outerVertices.Count; // 最初の穴の開始位置
            foreach (List<Vector3> hole in holeVertices)
            {
                GL.DrawArrays(PrimitiveType.TriangleFan, holeOffset, hole.Count);
                holeOffset += hole.Count; // 次の穴のためにオフセットを更新
            }

            // 3. カラーバッファへの書き込みを再有効化
            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);

            // ステンシルバッファを参照しながら描画（穴の部分は描画しない）
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);  // ステンシル値が1の部分だけ描画
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

            // 外側ポリゴンを再度描画（穴の部分を避けて描画）
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, outerVertices.Count);

            GL.BindVertexArray(0);

            // ステンシルバッファを無効化
            GL.Disable(EnableCap.StencilTest);
        }

        public void Dispose()
        {
            // VAOとVBOを解放
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }
    }
}
