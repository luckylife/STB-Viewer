using OpenTK.Mathematics;

namespace STBViewer2Lib.SettingWindow
{
    public class CategorySetting
    {
        public System.Windows.Media.Color SelectionColor { get; set; } = System.Windows.Media.Colors.Red;
        public System.Windows.Media.Color StbNodeColor { get; set; } = System.Windows.Media.Colors.Blue;
        public System.Windows.Media.Color StbColumnColor { get; set; } = System.Windows.Media.Colors.Yellow;
        public System.Windows.Media.Color StbPostColor { get; set; } = System.Windows.Media.Colors.Green;
        public System.Windows.Media.Color StbGirderColor { get; set; } = System.Windows.Media.Colors.Purple;
        public System.Windows.Media.Color StbBeamColor { get; set; } = System.Windows.Media.Colors.Pink;
        public System.Windows.Media.Color StbBraceColor { get; set; } = System.Windows.Media.Colors.Plum;
        public System.Windows.Media.Color StbSlabColor { get; set; } = System.Windows.Media.Colors.Gray;
        public System.Windows.Media.Color StbWallColor { get; set; } = System.Windows.Media.Colors.RosyBrown;
        public System.Windows.Media.Color StbParapetColor { get; set; } = System.Windows.Media.Colors.MidnightBlue;
        public System.Windows.Media.Color StbFoundationColumnColor { get; set; } = System.Windows.Media.Colors.LightBlue;
        public System.Windows.Media.Color StbFootingColor { get; set; } = System.Windows.Media.Colors.Salmon;
        public System.Windows.Media.Color StbStripFootingColor { get; set; } = System.Windows.Media.Colors.Aqua;
        public System.Windows.Media.Color StbPileColor { get; set; } = System.Windows.Media.Colors.Violet;

        public bool ShowStbNode { get; set; } = true;
        public bool ShowStbColumn { get; set; } = true;
        public bool ShowStbPost { get; set; } = true;
        public bool ShowStbGirder { get; set; } = true;
        public bool ShowStbBeam { get; set; } = true;
        public bool ShowStbBrace { get; set; } = true;
        public bool ShowStbSlab { get; set; } = true;
        public bool ShowStbWall { get; set; } = true;
        public bool ShowStbParapet { get; set; } = true;
        public bool ShowStbFoundationColumn { get; set; } = true;
        public bool ShowStbFooting { get; set; } = true;
        public bool ShowStbStripFooting { get; set; } = true;
        public bool ShowStbPile { get; set; } = true;

        public static Color4 FromMediaColor(System.Windows.Media.Color color)
        {
            return new Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }


    }
}
