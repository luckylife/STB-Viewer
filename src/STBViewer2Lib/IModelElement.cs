using OpenTK.Mathematics;
using STBViewer2Lib.DetailsWindow;
using STBViewer2Lib.MainWindow;
using STBViewer2Lib.OpenGL;
using STBViewer2Lib.SettingWindow;
using STBViewer2Lib.Shaders;
using System.Collections;
using System.Xml.Serialization;
namespace STBViewer2Lib
{
    public interface IModelElement : IRender
    {
        [XmlIgnore]
        List<IRender> OutlineModel { get; set; }

        [XmlIgnore]
        List<Sphere> AnalysisNodes { get; set; }

        [XmlIgnore]
        bool isEnable { get; set; }

        [XmlIgnore]
        List<IPropertyTab> Tabs { get; set; }

        void IRender.Render(Matrix4 view, Matrix4 projection)
        {
            if (isEnable == false)
            {
                return;
            }

            foreach (IRender model in OutlineModel)
            {
                model.Render(view, projection);
            }
            foreach (Sphere node in AnalysisNodes)
            {
                node.Render(view, projection);
            }
        }

        void IRender.SetColor(Color4 color)
        {
            foreach (IRender model in OutlineModel)
            {
                model.SetColor(color);
            }
            foreach (Sphere node in AnalysisNodes)
            {
                node.SetColor(color);
            }
        }

        void SetElementColor(CategorySetting settings)
        {
            Color4 color = GetElementColor(settings);
            foreach (IRender model in OutlineModel)
            {
                model.SetColor(color);
            }
            foreach (Sphere node in AnalysisNodes)
            {
                node.SetColor(color);
            }
        }

        Color4 GetElementColor(CategorySetting settings);

        void SetElementEnable(CategorySetting settings);

        Tuple<IEnumerable<Vector3>, IEnumerable<IEnumerable<Vector3>>> IRender.GetBoundingPoints()
        {
            // FoundationColumn以外は要素一つ
            return OutlineModel.ElementAt(0).GetBoundingPoints();
        }

        List<IPropertyTab> GetDetails()
        {
            return Tabs;
        }

        private bool IsSteelShape(string propertyName)
        {
            return propertyName is "shape" or "shape_H" or "shape_T" or "shape_X" or "shape_Y";
        }

        List<PropertyDetail> GetStbSecSteelProperties(string shape, IST_BRIDGE istBridge);

        List<PropertyDetail> GetPropertyDetail(object obj, IST_BRIDGE istBridge)
        {
            List<PropertyDetail> propertyDetails = [];
            try
            {
                System.Reflection.PropertyInfo[] properties = obj.GetType().GetProperties();

                foreach (System.Reflection.PropertyInfo prop in properties)
                {
                    // 値を取得
                    object value = prop.GetValue(obj, null);
                    if (value == null)
                    {
                        continue; // nullの場合はスキップ
                    }

                    if (prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true).Any())
                    {
                        // FieldSpecifiedプロパティがある場合はそれを見る
                        if (properties.Any(p => p.Name == $"{prop.Name}Specified"))
                        {
                            System.Reflection.PropertyInfo specifiedProp = properties.First(p => p.Name == $"{prop.Name}Specified");
                            bool specified = (bool)specifiedProp.GetValue(obj, null);
                            if (!specified)
                            {
                                continue; // FieldSpecifiedがfalseの場合はスキップ
                            }
                        }
                        if (!IsSteelShape(prop.Name))
                        {
                            propertyDetails.Add(new PropertyDetail(prop.Name, value.ToString()));
                        }
                        else
                        {
                            List<PropertyDetail> children = GetStbSecSteelProperties(value.ToString(), istBridge);
                            propertyDetails.Add(new PropertyDetail(prop.Name, value.ToString(), children));
                        }
                    }
                    else if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                    {
                        continue;
                    }
                    else if (typeof(List<IRender>).IsAssignableFrom(prop.PropertyType) || typeof(List<Sphere>).IsAssignableFrom(prop.PropertyType))
                    {
                        continue;
                    }
                    else
                    {
                        // その他の型(多分要素)は名前だけ表示
                        List<PropertyDetail> children = [];
                        if (value is IEnumerable enumerable)
                        {
                            foreach (object? item in enumerable)
                            {
                                children = GetPropertyDetail(item, istBridge);
                            }
                        }
                        else
                        {
                            children = GetPropertyDetail(value, istBridge);
                        }

                        if (children.Count > 0)
                        {
                            propertyDetails.Add(new PropertyDetail("+" + prop.Name, "", children));
                        }
                        else
                        {
                            propertyDetails.Add(new PropertyDetail("+" + prop.Name, ""));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = System.Windows.MessageBox.Show($"エラーが発生しました: {ex.Message}");
            }

            return propertyDetails;
        }

        List<IPropertyTab> GetAdditionalDetails(IST_BRIDGE stBridge);
        void InitilizeModel(IST_BRIDGE istBridge, ShaderLoader shader);
        void SetDetails(IST_BRIDGE istBridge)
        {
            List<PropertyMemberDetail> propertyDetails = [];
            try
            {
                System.Reflection.PropertyInfo[] properties = GetType().GetProperties();

                foreach (System.Reflection.PropertyInfo prop in properties)
                {
                    // 値を取得
                    object value = prop.GetValue(this, null);
                    if (value == null)
                    {
                        continue; // nullの場合はスキップ
                    }

                    if (prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true).Any())
                    {
                        // FieldSpecifiedプロパティがある場合はそれを見る
                        if (properties.Any(p => p.Name == $"{prop.Name}Specified"))
                        {
                            System.Reflection.PropertyInfo specifiedProp = properties.First(p => p.Name == $"{prop.Name}Specified");
                            bool specified = (bool)specifiedProp.GetValue(this, null);
                            if (!specified)
                            {
                                continue; // FieldSpecifiedがfalseの場合はスキップ
                            }
                        }

                        StbNodeProperties(prop, istBridge, propertyDetails);
                    }
                    else if (CheckStbNodeIdOrderProperties(prop, istBridge, propertyDetails))
                    {
                        // trueの場合関数の中で処理
                        continue;
                    }
                    else if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                    {
                        continue;
                    }
                    else if (CustomTreeNode.AreGenericTypesCompatible(typeof(List<IRender>), prop.PropertyType) || CustomTreeNode.AreGenericTypesCompatible(typeof(List<IPropertyTab>), prop.PropertyType))
                    {
                        continue;
                    }
                    else if (CheckStbWallOffsetList(prop, istBridge, propertyDetails))
                    {
                        // trueの場合関数の中で処理
                        continue;
                    }
                    else if (CheckStbSlabOffsetList(prop, istBridge, propertyDetails))
                    {
                        // trueの場合関数の中で処理
                        continue;
                    }
                    else if (CheckStbOpenIdList(prop, istBridge, propertyDetails))
                    {
                        // trueの場合関数の中で処理
                        continue;
                    }
                    else
                    {
                        // その他の型(多分要素)は名前だけ表示
                        propertyDetails.Add(new PropertyMemberDetail("+" + prop.Name, ""));
                    }
                }
            }
            catch (Exception ex)
            {
                _ = System.Windows.MessageBox.Show($"エラーが発生しました: {ex.Message}");
            }
            Tabs.Add(new PropertyTabMember("部材", propertyDetails));
            List<IPropertyTab> tabs = GetAdditionalDetails(istBridge);
            if (tabs.Count > 0)
            {
                Tabs.AddRange(tabs);
            }
        }
        void StbNodeProperties(System.Reflection.PropertyInfo prop, IST_BRIDGE istBridge, List<PropertyMemberDetail> propertyDetails);
        bool CheckStbNodeIdOrderProperties(System.Reflection.PropertyInfo prop, IST_BRIDGE istBridge, List<PropertyMemberDetail> propertyDetails);
        bool CheckStbWallOffsetList(System.Reflection.PropertyInfo prop, IST_BRIDGE istBridge, List<PropertyMemberDetail> propertyDetails);
        bool CheckStbSlabOffsetList(System.Reflection.PropertyInfo prop, IST_BRIDGE istBridge, List<PropertyMemberDetail> propertyDetails);
        bool CheckStbOpenIdList(System.Reflection.PropertyInfo prop, IST_BRIDGE istBridge, List<PropertyMemberDetail> propertyDetails);
    }
}
