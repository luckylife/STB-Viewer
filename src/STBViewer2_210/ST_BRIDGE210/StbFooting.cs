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
    public partial class StbFooting : IModelElement_210
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
            return CategorySetting.FromMediaColor(settings.StbFootingColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbFooting;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            StbNode bottom = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node);
            OutlineModel.Add(new Cube((float)(bottom.X + offset_X) * AbstractModelManager.ScaleFactor, (float)(bottom.Y + offset_Y) * AbstractModelManager.ScaleFactor, (float)(bottom.Z + level_bottom) * AbstractModelManager.ScaleFactor, 0.5f, (float)rotate, shader));
            AnalysisNodes.Add(new Sphere((float)bottom.X * AbstractModelManager.ScaleFactor, (float)bottom.Y * AbstractModelManager.ScaleFactor, (float)bottom.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
        }
        List<IPropertyTab> IModelElement.GetAdditionalDetails(IST_BRIDGE istBridge)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            List<IPropertyTab> tabs = [];
            List<PropertyDetail> properties = [];

            StbSecFoundation_RC rc = stBridge.StbModel.StbSections.StbSecFoundation_RC.First(s => s.id == id_section);
            properties = IModelElement_210.GetPropertyDetail(rc);
            tabs.Add(new PropertySection("断面", properties));
            return tabs;
        }
    }
}
