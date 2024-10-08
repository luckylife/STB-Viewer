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
    public partial class StbSlab : IModelElement_210
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
            return CategorySetting.FromMediaColor(settings.StbSlabColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbSlab;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            List<Vector3> vertices = [];
            List<List<Vector3>> holes = [];
            string[] ids = this.StbNodeIdOrder.Split(' ');
            for (int i = 0; i < ids.Length; i++)
            {
                StbNode node = stBridge.StbModel.StbNodes.First(n => n.id == ids[i]);
                if (StbSlabOffsetList == null)
                {
                    vertices.Add(new Vector3((float)node.X * AbstractModelManager.ScaleFactor, (float)node.Y * AbstractModelManager.ScaleFactor, (float)node.Z * AbstractModelManager.ScaleFactor));
                }
                else
                {
                    vertices.Add(new Vector3((float)(node.X + this.StbSlabOffsetList[i].offset_X) * AbstractModelManager.ScaleFactor, (float)(node.Y + this.StbSlabOffsetList[i].offset_Y) * AbstractModelManager.ScaleFactor, (float)(node.Z + this.StbSlabOffsetList[i].offset_Z) * AbstractModelManager.ScaleFactor));
                }
                AnalysisNodes.Add(new Sphere((float)node.X * AbstractModelManager.ScaleFactor, (float)node.Y * AbstractModelManager.ScaleFactor, (float)node.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
            }

            IEnumerable<StbOpenArrangement>? opens = stBridge.StbModel?.StbMembers?.StbOpenArrangements?.Where(n => n.id == this.id);
            if (opens != null && opens.Any())
            {
                if (vertices == null || vertices.Count < 3)
                {
                    throw new ArgumentException("verticesには少なくとも3つの点が必要です。");
                }

                // 1点目と2点目、最終点の座標
                Vector3 v1 = vertices[0]; // 1点目
                Vector3 v2 = vertices[1]; // 2点目
                Vector3 vn = vertices[^1]; // 最終点

                // XVectorを定義（1点目から2点目へのベクトル）
                Vector3 XVector = (v2 - v1).Normalized();

                // 1点目、2点目、最終点が定義する平面の法線ベクトルを計算
                Vector3 normalVector = Vector3.Cross(v2 - v1, vn - v1).Normalized();

                // XVectorと法線ベクトルから、YVectorを計算
                Vector3 YVector = Vector3.Cross(normalVector, XVector).Normalized();

                // 1点目から最終点に向かうベクトルとYVectorの内積が正の方向か確認
                if (Vector3.Dot(YVector, vn - v1) < 0)
                {
                    // YVectorの方向が負の場合、反転させる
                    YVector = -YVector;
                }

                List<Vector3> hole = [];
                foreach (StbOpenArrangement open in opens)
                {
                    Vector3 first = vertices[0] + (XVector * (float)open.position_X * AbstractModelManager.ScaleFactor) + (YVector * (float)open.position_Y * AbstractModelManager.ScaleFactor);
                    StbSecOpen_RC? section = stBridge.StbModel?.StbSections?.StbSecOpen_RC?.FirstOrDefault(s => s.id == open.id_section);
                    if (section == null)
                    {
                        // エラーメッセージ
                    }
                    else
                    {
                        Vector3 second = first + (XVector * (float)section.length_X * AbstractModelManager.ScaleFactor);
                        Vector3 third = second + (YVector * (float)section.length_Y * AbstractModelManager.ScaleFactor);
                        Vector3 fourth = first + (YVector * (float)section.length_Y * AbstractModelManager.ScaleFactor);
                        holes.Add([first, second, third, fourth]);
                    }
                }
            }

            OutlineModel.Add(new Plane(vertices, holes, shader)); // スケール変換後の座標で初期化
        }

        List<IPropertyTab> IModelElement.GetAdditionalDetails(IST_BRIDGE istBridge)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            List<IPropertyTab> tabs = [];
            List<PropertyDetail> properties = [];
            if (kind_structure.ToString() == "RC")
            {
                StbSecSlab_RC rc = stBridge.StbModel.StbSections.StbSecSlab_RC.First(s => s.id == id_section);
                properties = ((IModelElement)(this)).GetPropertyDetail(rc, istBridge);
            }
            else if (kind_structure.ToString() == "DECK")
            {
                StbSecSlabDeck s = stBridge.StbModel.StbSections.StbSecSlabDeck.First(s => s.id == id_section);
                properties = ((IModelElement)(this)).GetPropertyDetail(s, istBridge);
            }
            else if (kind_structure.ToString() == "PRECAST")
            {
                StbSecParapet_RC src = stBridge.StbModel.StbSections.StbSecParapet_RC.First(s => s.id == id_section);
                properties = ((IModelElement)(this)).GetPropertyDetail(src, istBridge);
            }
            tabs.Add(new PropertySection("断面", properties));


            /*
            if (StbOpenIdList != null)
            {
                List<PropertyDetail> holes = [];
                int index = 0;
                foreach (StbOpenId? id in StbOpenIdList)
                {
                    StbOpen open = stBridge.StbModel.StbMembers.StbOpens.First(o => o.id == id.id);
                    PropertyDetail property = new("StbOpen", index.ToString())
                    {
                        Children = IModelElement_210.GetPropertyDetail(open)
                    };
                    for (int i = 0; i < property.Children.Count(); i++)
                    {
                        if (property.Children.ElementAt(i).PropertyName == "id_section")
                        {
                            StbSecOpen_RC secOpen = stBridge.StbModel.StbSections.StbSecOpen_RC.First(s => s.id == open.id_section);
                            property.Children.ElementAt(i).Children = IModelElement_210.GetPropertyDetail(secOpen);
                        }
                    }

                    holes.Add(property);
                    index++;
                }
                tabs.Add(new PropertySection("開口", holes));
            }
            */
            return tabs;
        }
    }
}
