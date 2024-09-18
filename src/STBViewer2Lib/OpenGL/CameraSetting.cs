using OpenTK.Mathematics;

namespace STBViewer2Lib.OpenGL
{
    public partial class CameraSetting
    {
        public List<string> FloorGrids() { return floorGrids.Select(n => n.Name).ToList(); }

        public Vector3 CameraTarget { get; set; }

        // カメラからターゲットへの方向ベクトル。カメラターゲットを含む平面の法線方向ベクトルでもある。
        public Vector3 CameraDirection { get; set; }

        public bool IsOrtho { get; set; }
        public OrthoSetting orthoSetting { get; set; } = new OrthoSetting();
        public PerspectiveSetting perspectiveSetting { get; set; } = new PerspectiveSetting();

        private Vector3 modelMin;
        private Vector3 modelMax;
        private List<FloorGrid> floorGrids = [];

        public void SetOrtho(int index, bool isInvert)
        {
            if (!IsOrtho)
            {
                return;
            }

            if (0 <= index && index < floorGrids.Count)
            {
                CameraTarget = floorGrids[index].Target;
                CameraDirection = !isInvert ? floorGrids[index].Direction : -floorGrids[index].Direction;
                orthoSetting.UpDirection = floorGrids[index].UpDirection;
                orthoSetting.Ratio = 0.7f;
            }
        }

        public void SetDefault()
        {
            // モデルの中心点を計算
            float centerX = (float)((modelMin.X + modelMax.X) / 2);
            float centerY = (float)((modelMin.Y + modelMax.Y) / 2);
            float centerZ = (float)((modelMin.Z + modelMax.Z) / 2);

            CameraTarget = new Vector3(centerX, centerY, centerZ);
            CameraDirection = new Vector3(-1, 1, -1).Normalized();
            IsOrtho = false;
            OrthoSetting ortho = new()
            {
                Near = 1000f,
                Far = 1000f,
                Ratio = 0.7f,
                UpDirection = new Vector3(0, 1, 0)
            };
            PerspectiveSetting perspectiveSetting = new()
            {
                CameraDistance = (float)Math.Max(Math.Max(modelMax.X - modelMin.X, modelMax.Y - modelMin.Y), modelMax.Z - modelMin.Z) * 1.5f, // モデルサイズに基づいたカメラ距離
                ZoomFactor = 1.0f,
                NearClip = 0.1f,   // 手前のクリップ面
                FarClip = 1000.0f  // 奥のクリップ面
            };
            orthoSetting = ortho;
            this.perspectiveSetting = perspectiveSetting;
        }

        public CameraSetting Clone()
        {
            return new CameraSetting
            {
                CameraTarget = CameraTarget,
                CameraDirection = CameraDirection,
                IsOrtho = IsOrtho,
                orthoSetting = orthoSetting.Clone(),
                perspectiveSetting = perspectiveSetting.Clone(),
                modelMin = modelMin,
                modelMax = modelMax,
                floorGrids = floorGrids.Select(n => n.Clone()).ToList()
            };
        }

        public void SetST_Bridge(IST_BRIDGE istBridge)
        {
            if (istBridge == null || !istBridge.HasStbNodes())
            {
                return;
            }
            (Vector3, Vector3) minMax = istBridge.GetMinMax();
            modelMin = minMax.Item1;
            modelMax = minMax.Item2;
            floorGrids.Clear();
            floorGrids.AddRange(istBridge.MakeStories(modelMin, modelMax));
            floorGrids.AddRange(istBridge.MakeParallelAxes(modelMin, modelMax));
            floorGrids.AddRange(istBridge.MakeRadialAxes(modelMin, modelMax));
            SetDefault();
        }

        public Tuple<float, float> GetMax()
        {
            return CameraDirection == new Vector3(0, 0, -1)
                ? new Tuple<float, float>(modelMax.X - modelMin.X, modelMax.Y - modelMin.Y)
                : new Tuple<float, float>(Math.Max(modelMax.X - modelMin.X, modelMax.Y - modelMin.Y), modelMax.Z - modelMin.Z);
        }

    }
    public class OrthoSetting
    {
        // カメラターゲットを含む平面からのカメラに向かう方向の距離(mm)
        public float Near { get; set; }

        // カメラターゲットを含む平面からのカメラから離れる方向の距離(mm)
        public float Far { get; set; }

        // 幅または高さが、画面のRatio倍になるように調整するための係数
        public float Ratio { get; set; }


        // 紙面の上方向。階平面の場合はY軸正方向。それ以外の場合はZ軸正方向。
        public Vector3 UpDirection { get; set; }

        public OrthoSetting Clone()
        {
            return new OrthoSetting
            {
                Near = Near,
                Far = Far,
                Ratio = Ratio,
                UpDirection = UpDirection
            };
        }
    }
    public class PerspectiveSetting
    {
        // カメラの距離(mm)
        public float CameraDistance { get; set; }

        public float ZoomFactor { get; set; }

        public float NearClip { get; set; } = 0.1f;
        public float FarClip { get; set; } = 1000.0f;

        public PerspectiveSetting Clone()
        {
            return new PerspectiveSetting
            {
                CameraDistance = CameraDistance,
                ZoomFactor = ZoomFactor,
                NearClip = NearClip,
                FarClip = FarClip
            };
        }
    }
}