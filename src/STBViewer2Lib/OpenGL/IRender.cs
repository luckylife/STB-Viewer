using OpenTK.Mathematics;
namespace STBViewer2Lib.OpenGL
{
    public interface IRender
    {
        // 描画メソッド
        void Render(Matrix4 view, Matrix4 projection);

        // モデルの代表点を取得するメソッド（スクリーン投影に使う）
        // 外側のポリゴンの頂点リストと、穴のポリゴンのリストを返す
        Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> GetBoundingPoints();

        // 色を変更するメソッド
        void SetColor(Color4 color);

        // 選択状態のプロパティ
        bool isSelected { get; set; }
    }
}
