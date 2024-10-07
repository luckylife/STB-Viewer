using OpenTK.Mathematics;
using STBViewer2_210.ST_BRIDGE210;
using STBViewer2Lib;
using STBViewer2Lib.DetailsWindow;
using STBViewer2Lib.MainWindow;
using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;
using STBViewer2Lib.Shaders;
using System.Xml.Serialization;

namespace ST_BRIDGE210
{
    public partial class StbPile : IModelElement_210
    {
        [XmlIgnore]
        public List<IRender> OutlineModel { get; set; } = [];

        [XmlIgnore]
        public bool isSelected { get; set; }

        [XmlIgnore]
        public bool isEnable { get; set; }

        [XmlIgnore]
        public List<Sphere> AnalysisNodes { get; set; } = [];

        [XmlIgnore]
        public List<IPropertyTab> Tabs { get; set; } = [];

        public Color4 GetElementColor(CategorySetting settings)
        {
            return CategorySetting.FromMediaColor(settings.StbPileColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbPile;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            StbNode top = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node);
            Vector3 offsetTop = new((float)(top.X + offset_X), (float)(top.Y + offset_Y), (float)(top.Z + level_top));
            Vector3 offsetBottom = offsetTop - new Vector3(0, 0, (float)length_all);

            OutlineModel.Add(new Wire(new Vector3(offsetTop.X, offsetTop.Y, offsetTop.Z) * AbstractModelManager.ScaleFactor,
                new Vector3(offsetBottom.X, offsetBottom.Y, offsetBottom.Z) * AbstractModelManager.ScaleFactor, shader));
            AnalysisNodes.Add(new Sphere((float)top.X * AbstractModelManager.ScaleFactor, (float)top.Y * AbstractModelManager.ScaleFactor, (float)top.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
        }

        List<IPropertyTab> IModelElement.GetAdditionalDetails(IST_BRIDGE istBridge)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            List<IPropertyTab> tabs = [];
            List<PropertyDetail> properties = [];
            if (kind_structure.ToString() == "RC")
            {
                StbSecPile_RC rc = stBridge.StbModel.StbSections.StbSecPile_RC.First(s => s.id == id_section);
                properties = IModelElement_210.GetPropertyDetail(rc);
            }
            else if (kind_structure.ToString() == "S")
            {
                StbSecPile_S s = stBridge.StbModel.StbSections.StbSecPile_S.First(s => s.id == id_section);
                properties = IModelElement_210.GetPropertyDetail(s);
            }
            else if (kind_structure.ToString() == "PC")
            {
                StbSecPilePrecast src = stBridge.StbModel.StbSections.StbSecPilePrecast.First(s => s.id == id_section);
                properties = IModelElement_210.GetPropertyDetail(src);
            }
            tabs.Add(new PropertySection("断面", properties));
            return tabs;
        }
    }
}