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
    public partial class StbNode : IModelElement_210
    {
        [XmlIgnore]
        public List<IRender> OutlineModel { get; set; } = [];

        [XmlIgnore]
        public List<Sphere> AnalysisNodes { get; set; } = [];

        [XmlIgnore]
        public bool isSelected { get; set; }

        [XmlIgnore]
        public bool isEnable { get; set; }

        [XmlIgnore]
        public List<IPropertyTab> Tabs { get; set; } = [];

        public Color4 GetElementColor(CategorySetting settings)
        {
            return CategorySetting.FromMediaColor(settings.StbNodeColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbNode;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            OutlineModel.Add(new Sphere((float)X * AbstractModelManager.ScaleFactor, (float)Y * AbstractModelManager.ScaleFactor, (float)Z * AbstractModelManager.ScaleFactor, 0.1f, shader)); // スケール変換後の座標で初期化
        }

        public List<IPropertyTab> GetAdditionalDetails(IST_BRIDGE stBridge)
        {
            throw new NotImplementedException();
        }

        List<IPropertyTab> IModelElement.GetAdditionalDetails(IST_BRIDGE istBridge)
        {
            return [];
        }
    }
}
