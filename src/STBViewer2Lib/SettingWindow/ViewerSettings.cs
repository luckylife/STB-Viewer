using STBViewer2Lib.OpenGL;

namespace STBViewer2Lib.SettingWindow
{
    public class ViewerSettings
    {
        public bool isSync { get; set; }
        public bool isInvert { get; set; }
        public List<string> floorGrids { get; set; } = [];
        public int index { get; set; }


        public CategorySetting categorySetting { get; set; } = new();
        private CameraSetting cameraSetting = new();

        public void SetCameraSetting(bool isOrth, int index, string top, string depth)
        {
            cameraSetting.IsOrtho = isOrth;
            this.index = index;
            cameraSetting.orthoSetting.Near = float.Parse(top);
            cameraSetting.orthoSetting.Far = float.Parse(depth);
        }

        public void SetST_Bridge(IST_BRIDGE stBridge)
        {
            cameraSetting.SetST_Bridge(stBridge);
            floorGrids = cameraSetting.FloorGrids();
        }

        public CameraSetting CameraSetting()
        {
            cameraSetting.SetOrtho(index, isInvert);
            return cameraSetting;
        }

        public ViewerSettings Clone()
        {
            return (ViewerSettings)MemberwiseClone();
        }

        public void DeleteOrthoViews()
        {
            floorGrids.Clear();
            cameraSetting = new CameraSetting();
        }

    }
}
