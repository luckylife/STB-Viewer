using ST_BRIDGE202;
using STBViewer2Lib;
using STBViewer2Lib.DetailsWindow;
using System.Text;
namespace STBViewer2_202.ST_BRIDGE202
{

    public interface IModelElement_202 : IModelElement
    {
        void IModelElement.StbNodeProperties(System.Reflection.PropertyInfo prop, IST_BRIDGE istBridge, List<PropertyMemberDetail> propertyDetails)
        {
            object value = prop.GetValue(this, null);
            ST_BRIDGE stBridge = istBridge as ST_BRIDGE;

            List<string> children = [];
            if (prop.Name is "id_node" or "id_node_start" or "id_node_end" or "id_node_bottom" or "id_node_top")
            {
                if (stBridge?.StbModel?.StbNodes?.FirstOrDefault(n => n.id == (string)value) is StbNode node)
                {
                    children.Add($"(X:{node.X}, Y:{node.Y}, Z:{node.Z})");
                }
            }

            if (children.Count > 0)
            {
                propertyDetails.Add(new PropertyMemberDetail(prop.Name, value.ToString(), children));
            }
            else
            {
                propertyDetails.Add(new PropertyMemberDetail(prop.Name, value.ToString()));
            }
        }

        bool IModelElement.CheckStbNodeIdOrderProperties(System.Reflection.PropertyInfo prop, STBViewer2Lib.IST_BRIDGE istBridge, System.Collections.Generic.List<STBViewer2Lib.DetailsWindow.PropertyMemberDetail> propertyDetails)
        {
            if (prop.Name != "StbNodeIdOrder")
            {
                return false;
            }

            object value = prop.GetValue(this, null);
            ST_BRIDGE stBridge = istBridge as ST_BRIDGE;

            List<string> children = [];
            foreach (string item in value.ToString().Split(' '))
            {
                StbNode node = stBridge?.StbModel?.StbNodes?.FirstOrDefault(n => n.id == item);
                children.Add($"(id:{item}, X:{node.X}, Y:{node.Y}, Z:{node.Z})");
            }
            propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, value.ToString(), children));
            return true;
        }

        bool IModelElement.CheckStbWallOffsetList(System.Reflection.PropertyInfo prop, STBViewer2Lib.IST_BRIDGE istBridge, System.Collections.Generic.List<STBViewer2Lib.DetailsWindow.PropertyMemberDetail> propertyDetails)
        {
            if (prop.Name != "StbWallOffsetList")
            {
                return false;
            }

            object value = prop.GetValue(this, null);
            ST_BRIDGE stBridge = istBridge as ST_BRIDGE;

            List<string> children = [];
            if (value is StbWallOffset[] offsetList)
            {
                foreach (StbWallOffset? offset in offsetList)
                {
                    StbNode node = stBridge?.StbModel?.StbNodes?.FirstOrDefault(n => n.id == offset.id_node);
                    StringBuilder sb = new();
                    _ = sb.Append($"(id:{node.id}");
                    _ = sb.Append($", offsetX:{offset.offset_X}");
                    _ = sb.Append($", offsetY:{offset.offset_Y}");
                    _ = sb.Append($", offsetZ:{offset.offset_Z})");
                    children.Add(sb.ToString());
                }
            }

            if (children.Count > 0)
            {
                propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, "", children));
            }
            else
            {
                propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, ""));
            }
            return true;
        }
        bool IModelElement.CheckStbSlabOffsetList(System.Reflection.PropertyInfo prop, STBViewer2Lib.IST_BRIDGE istBridge, System.Collections.Generic.List<STBViewer2Lib.DetailsWindow.PropertyMemberDetail> propertyDetails)
        {
            if (prop.Name != "StbSlabOffsetList")
            {
                return false;
            }

            object value = prop.GetValue(this, null);
            ST_BRIDGE stBridge = istBridge as ST_BRIDGE;

            List<string> children = [];
            if (value is StbSlabOffset[] offsetList)
            {
                foreach (StbSlabOffset? offset in offsetList)
                {
                    StbNode node = stBridge?.StbModel?.StbNodes?.FirstOrDefault(n => n.id == offset.id_node);
                    StringBuilder sb = new();
                    _ = sb.Append($"(id:{node.id}");
                    _ = sb.Append($", offsetX:{offset.offset_X}");
                    _ = sb.Append($", offsetY:{offset.offset_Y}");
                    _ = sb.Append($", offsetZ:{offset.offset_Z})");
                    children.Add(sb.ToString());
                }
            }
            if (children.Count > 0)
            {
                propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, "", children));
            }
            else
            {
                propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, ""));
            }
            return true;
        }
        bool IModelElement.CheckStbOpenIdList(System.Reflection.PropertyInfo prop, STBViewer2Lib.IST_BRIDGE istBridge, System.Collections.Generic.List<STBViewer2Lib.DetailsWindow.PropertyMemberDetail> propertyDetails)
        {
            if (prop.Name != "StbOpenIdList")
            {
                return false;
            }

            object value = prop.GetValue(this, null);
            ST_BRIDGE stBridge = istBridge as ST_BRIDGE;

            List<string> children = [];
            if (value is StbOpenId[] openList)
            {
                children.Add(string.Join(",", openList.Select(n => n.id)));
            }
            if (children.Count > 0)
            {
                propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, "", children));
            }
            else
            {
                propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, ""));
            }

            return true;
        }
    }
}
