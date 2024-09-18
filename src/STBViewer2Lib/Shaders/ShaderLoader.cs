using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace STBViewer2Lib.Shaders
{
    public class ShaderLoader
    {
        public int Handle { get; private set; }

        public ShaderLoader(string vertexPath, string fragmentPath)
        {
            // シェーダーソースをファイルから読み込み
            string vertexShaderSource = LoadShader(vertexPath);
            string fragmentShaderSource = LoadShader(fragmentPath);

            // 頂点シェーダーのコンパイル
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            CompileShader(vertexShader);

            // フラグメントシェーダーのコンパイル
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

            // シェーダープログラムのリンク
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            // シェーダーの削除（リンク後は不要）
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // リンクのエラーチェック
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Console.WriteLine($"Error linking shader program: {infoLog}");
            }
        }

        // 埋め込まれたシェーダーを読み込むメソッド
        public string LoadShader(string shaderName)
        {
            // 現在のアセンブリからリソースを取得
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"STBViewer2Lib.Shaders.{shaderName}"; // リソース名をアセンブリ内のパスに合わせて設定

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException($"Shader {shaderName} not found in embedded resources.");
            }

            using StreamReader reader = new(stream, Encoding.UTF8);
            return reader.ReadToEnd(); // シェーダーの内容を文字列として返す
        }

        private void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            // コンパイルエラーチェック
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"Error compiling shader: {infoLog}");
            }
        }

        // シェーダープログラムを使用
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // シェーダープログラムの削除
        public void Delete()
        {
            GL.DeleteProgram(Handle);
        }
    }
}