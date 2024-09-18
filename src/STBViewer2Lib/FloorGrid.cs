using OpenTK.Mathematics;
using STBViewer2Lib.MainWindow;

namespace STBViewer2Lib
{
    public class FloorGrid
    {
        public string Name { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 UpDirection { get; set; }

        private FloorGrid() { }
        public FloorGrid(string storyName, double height, Vector3 modelMin, Vector3 modelMax)
        {
            Name = "階：" + storyName;
            UpDirection = new Vector3(0, 1, 0);
            Target = new Vector3((float)((modelMin.X + modelMax.X) / 2), (float)((modelMin.Y + modelMax.Y) / 2), (float)(height * AbstractModelManager.ScaleFactor));
            Direction = new Vector3(0, 0, -1);
        }

        public FloorGrid(Vector2 axes, double angle, string name, double distance, Vector3 modelMin, Vector3 modelMax)
        {
            // axesの中心
            double offsetX = axes.X * AbstractModelManager.ScaleFactor;
            double offsetY = axes.Y * AbstractModelManager.ScaleFactor;

            // 90度回転
            double offsetTheta = (angle + 90) * Math.PI / 180;
            Direction = new Vector3((float)Math.Cos(offsetTheta), (float)Math.Sin(offsetTheta), 0);

            // オフセットした点
            offsetX += Math.Cos(offsetTheta) * distance * AbstractModelManager.ScaleFactor;
            offsetY += Math.Sin(offsetTheta) * distance * AbstractModelManager.ScaleFactor;

            // 角度をラジアンに変換
            double theta = angle * Math.PI / 180;

            // 方向ベクトル
            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            // 中心に近い点
            double centerX = (modelMin.X + modelMax.X) / 2;
            double centerY = (modelMin.Y + modelMax.Y) / 2;

            // tを計算
            double t = ((centerX - offsetX) * cosTheta) + ((centerY - offsetY) * sinTheta);

            // 最近接点の座標を計算
            double targetX = offsetX + (t * cosTheta);
            double targetY = offsetY + (t * sinTheta);
            double targetZ = (modelMin.Z + modelMax.Z) / 2;
            Target = new Vector3((float)targetX, (float)targetY, (float)targetZ);

            Name = "平行軸：" + name;
            UpDirection = new Vector3(0, 0, 1);
        }

        public FloorGrid(Vector2 axes, string name, double angle, Vector3 modelMin, Vector3 modelMax)
        {
            double offsetX = axes.X * AbstractModelManager.ScaleFactor;
            double offsetY = axes.Y * AbstractModelManager.ScaleFactor;

            double offsetTheta = (angle + 90) * Math.PI / 180;
            Direction = new Vector3((float)Math.Cos(offsetTheta), (float)Math.Sin(offsetTheta), 0);

            // 角度をラジアンに変換
            double theta = angle * Math.PI / 180;

            // 方向ベクトル
            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            // 中心に近い点
            double centerX = (modelMin.X + modelMax.X) / 2;
            double centerY = (modelMin.Y + modelMax.Y) / 2;

            // tを計算
            double t = ((centerX - offsetX) * cosTheta) + ((centerY - offsetY) * sinTheta);

            // 最近接点の座標を計算
            double targetX = offsetX + (t * cosTheta);
            double targetY = offsetY + (t * sinTheta);
            double targetZ = (modelMin.Z + modelMax.Z) / 2;
            Target = new Vector3((float)targetX, (float)targetY, (float)targetZ);

            Name = "放射軸：" + name;
            UpDirection = new Vector3(0, 0, 1);
        }

        public FloorGrid Clone()
        {
            return new FloorGrid
            {
                Name = Name,
                Target = Target,
                Direction = Direction,
                UpDirection = UpDirection
            };
        }

    }
}