using OpenTK.Mathematics;
using STBViewer2_201;
using STBViewer2_201.MainWindow;
using STBViewer2_201.ST_BRIDGE201;
using STBViewer2Lib;
using STBViewer2Lib.DetailsWindow;
using STBViewer2Lib.MainWindow;
using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;
using STBViewer2Lib.Shaders;
using System.Xml.Serialization;

namespace ST_BRIDGE201
{
    public partial class StbFoundationColumn : IModelElement_201
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
            return CategorySetting.FromMediaColor(settings.StbFoundationColumnColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbFoundationColumn;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            StbNode node = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node);
            Vector3 offsetFD = new((float)(node.X + offset_FD_X), (float)(node.Y + offset_FD_Y), (float)(node.Z + offset_Z));
            Vector3 offsetFDBottom = offsetFD - new Vector3(0, 0, (float)length_FD);

            Vector3 offsetWR = new((float)(node.X + offset_WR_X), (float)(node.Y + offset_WR_Y), (float)(node.Z + offset_Z));
            Vector3 offsetWRTop = offsetWR + new Vector3(0, 0, (float)length_WR);


            OutlineModel.Add(new Wire(offsetFDBottom * AbstractModelManager.ScaleFactor, offsetFD * AbstractModelManager.ScaleFactor, shader));
            OutlineModel.Add(new Wire(offsetWR * AbstractModelManager.ScaleFactor, offsetWRTop * AbstractModelManager.ScaleFactor, shader));
            AnalysisNodes.Add(new Sphere((float)node.X * AbstractModelManager.ScaleFactor, (float)node.Y * AbstractModelManager.ScaleFactor, (float)node.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
        }

        Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> IRender.GetBoundingPoints()
        {
            IEnumerable<Vector3> first = OutlineModel.ElementAt(0).GetBoundingPoints().Item1;
            IEnumerable<Vector3> second = OutlineModel.ElementAt(1).GetBoundingPoints().Item1;

            // 最初と最後を結ぶ線分としておく
            return new Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>>([first.ElementAt(0), second.ElementAt(1)], new List<List<Vector3>>());
        }

        List<IPropertyTab> IModelElement.GetAdditionalDetails(IST_BRIDGE istBridge)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            List<IPropertyTab> tabs = [];
            List<PropertyDetail> properties = [];
            if (kind_structure.ToString() == "RC")
            {
                if (id_section_FD != null)
                {
                    StbSecColumn_RC rc = stBridge.StbModel.StbSections.StbSecColumn_RC.First(s => s.id == id_section_FD);
                    properties.AddRange(IModelElement_201.GetPropertyDetail(rc));
                }

                if (id_section_WR != null)
                {
                    StbSecColumn_RC rc = stBridge.StbModel.StbSections.StbSecColumn_RC.First(s => s.id == id_section_WR);
                    properties.AddRange(IModelElement_201.GetPropertyDetail(rc));
                }
            }
            tabs.Add(new PropertySection("断面", properties));
            return tabs;
        }

    }
}