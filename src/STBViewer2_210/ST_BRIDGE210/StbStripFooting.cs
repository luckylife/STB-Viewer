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
    public partial class StbStripFooting : IModelElement_210
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
            return CategorySetting.FromMediaColor(settings.StbStripFootingColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbStripFooting;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            StbNode start = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node_start);
            StbNode end = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node_end);
            Vector3 direction = new Vector3((float)end.X, (float)end.Y, (float)end.Z) - new Vector3((float)start.X, (float)start.Y, (float)start.Z);
            Vector3 normalizedOffset = Vector3.Normalize(Vector3.Cross(direction, new Vector3(0, 0, 1)));

            OutlineModel.Add(new Wire(new Vector3((float)(start.X + (this.offset * normalizedOffset.X)) * AbstractModelManager.ScaleFactor, (float)(start.Y + (offset * normalizedOffset.Y)) * AbstractModelManager.ScaleFactor, (float)(start.Z + (offset * normalizedOffset.Z)) * AbstractModelManager.ScaleFactor),
                new Vector3((float)(end.X + (offset * normalizedOffset.X)) * AbstractModelManager.ScaleFactor, (float)(end.Y + (offset * normalizedOffset.Y)) * AbstractModelManager.ScaleFactor, (float)(end.Z + (offset * normalizedOffset.Z)) * AbstractModelManager.ScaleFactor), shader)); // スケール変換後の座標で初期化
            AnalysisNodes.Add(new Sphere((float)start.X * AbstractModelManager.ScaleFactor, (float)start.Y * AbstractModelManager.ScaleFactor, (float)start.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
            AnalysisNodes.Add(new Sphere((float)end.X * AbstractModelManager.ScaleFactor, (float)end.Y * AbstractModelManager.ScaleFactor, (float)end.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
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
