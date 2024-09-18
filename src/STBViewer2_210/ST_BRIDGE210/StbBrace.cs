﻿using OpenTK.Mathematics;
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
    public partial class StbBrace : IModelElement_210
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
            return CategorySetting.FromMediaColor(settings.StbBraceColor);
        }

        public void SetElementEnable(CategorySetting settings)
        {
            isEnable = settings.ShowStbBrace;
        }

        public void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader)
        {
            // TODO cutbackの実装

            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            StbNode bottom = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node_start);
            StbNode top = stBridge.StbModel.StbNodes.First(n => n.id == this.id_node_end);
            OutlineModel.Add(new Wire(new Vector3((float)(bottom.X + this.aim_offset_start_X) * AbstractModelManager.ScaleFactor, (float)(bottom.Y + aim_offset_start_Y) * AbstractModelManager.ScaleFactor, (float)(bottom.Z + aim_offset_start_Z) * AbstractModelManager.ScaleFactor),
                new Vector3((float)(top.X + aim_offset_end_X) * AbstractModelManager.ScaleFactor, (float)(top.Y + aim_offset_end_Y) * AbstractModelManager.ScaleFactor, (float)(top.Z + aim_offset_end_Z) * AbstractModelManager.ScaleFactor), shader)); // スケール変換後の座標で初期化
            AnalysisNodes.Add(new Sphere((float)bottom.X * AbstractModelManager.ScaleFactor, (float)bottom.Y * AbstractModelManager.ScaleFactor, (float)bottom.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
            AnalysisNodes.Add(new Sphere((float)top.X * AbstractModelManager.ScaleFactor, (float)top.Y * AbstractModelManager.ScaleFactor, (float)top.Z * AbstractModelManager.ScaleFactor, 0.1f, shader));
        }

        List<IPropertyTab> GetAdditionalDetails(IST_BRIDGE istBridge)
        {
            ST_BRIDGE? stBridge = istBridge as ST_BRIDGE;
            List<IPropertyTab> tabs = [];
            List<PropertyDetail> properties = [];
            if (kind_structure.ToString() == "S")
            {
                StbSecBrace_S s = stBridge.StbModel.StbSections.StbSecBrace_S.First(s => s.id == id_section);
                properties = IModelElement_210.GetPropertyDetail(s);
            }
            tabs.Add(new PropertySection("断面", properties));
            /*
            if (joint_id_start != null)
            {
                List<PropertyDetail> jointProperties = [];
                StbJointBeamShapeH start = stBridge.StbModel.StbJoints.StbJointBeamShapeH.First(j => j.id == joint_id_start);
                jointProperties.AddRange(IModelElement_210.GetPropertyDetail(start));
                tabs.Add(new PropertySection("継手始端", jointProperties));
            }
            if (joint_id_end != null)
            {
                List<PropertyDetail> jointProperties = [];
                StbJointBeamShapeH end = stBridge.StbModel.StbJoints.StbJointBeamShapeH.First(j => j.id == joint_id_end);
                jointProperties.AddRange(IModelElement_210.GetPropertyDetail(end));
                tabs.Add(new PropertySection("継手終端", jointProperties));
            }
            */
            return tabs;
        }

    }
}
