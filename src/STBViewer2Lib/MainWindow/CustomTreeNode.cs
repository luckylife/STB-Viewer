using STBViewer2Lib.DetailsWindow;
using STBViewer2Lib.OpenGL;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace STBViewer2Lib.MainWindow
{
    public class CustomTreeNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Name { get; }
        public IModelElement ModelElement { get; }

        private ObservableCollection<CustomTreeNode> _children = [];
        public ObservableCollection<CustomTreeNode> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }

        public string Details { get; }  // 属性情報を保持するプロパティ
                                        // 対応する3Dモデルの要素

        // コンストラクタで属性情報を生成
        private CustomTreeNode(object obj, string nodeName, IModelElement? modelElement = null)
        {
            Name = nodeName == "" ? obj.GetType().Name : nodeName;
            Children = [];
            Details = GenerateDetailsString(obj);
            ModelElement = modelElement;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // 属性情報を生成するメソッド
        private string GenerateDetailsString(object obj)
        {
            try
            {
                StringBuilder details = new();

                _ = details.AppendLine($"Details of {Name}:");
                _ = details.AppendLine(new string('-', 40));

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(obj.GetType()) && obj is System.Collections.IEnumerable collection) //&& collection is not StbNodeId[])
                {

                    // コレクション型の場合、要素の型と数を表示
                    // 値を取得
                    int count = 0;
                    Type? elementType = null;

                    foreach (object? item in collection)
                    {
                        if (elementType == null)
                        {
                            elementType = item?.GetType(); // 最初の要素の型を取得
                        }
                        count++; // 要素の数をカウント
                    }

                    _ = details.AppendLine($"{elementType.Name}[{count}] ");
                }
                else
                {
                    PropertyInfo[] properties = obj.GetType().GetProperties();
                    foreach (PropertyInfo prop in properties)
                    {
                        if (prop.Name is "SyncRoot" or "IsReadOnly" or "IsFixedSize" or "IsSynchronized")
                        {
                            continue; // ICollectionのプロパティはスキップ
                        }

                        if (prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true).Any())
                        {
                            // 値を取得
                            object value = prop.GetValue(obj, null);
                            if (value == null)
                            {
                                continue; // nullの場合はスキップ
                            }

                            // FieldSpecifiedプロパティがある場合はそれを見る
                            if (properties.Any(p => p.Name == $"{prop.Name}Specified"))
                            {
                                PropertyInfo specifiedProp = properties.First(p => p.Name == $"{prop.Name}Specified");
                                bool specified = (bool)specifiedProp.GetValue(obj, null);
                                if (!specified)
                                {
                                    continue; // FieldSpecifiedがfalseの場合はスキップ
                                }
                            }

                            _ = details.AppendLine($"   {prop.Name}: {value}");
                        }
                        else if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                        {
                            continue;
                        }
                        else if (AreGenericTypesCompatible(typeof(List<IRender>), prop.PropertyType) || AreGenericTypesCompatible(typeof(List<IPropertyTab>), prop.PropertyType))
                        {
                            continue;
                        }
                        else
                        {
                            // その他の型(多分要素)は名前だけ表示
                            _ = details.AppendLine($"+{prop.Name}");
                        }
                    }
                }

                // フォーマットの最後に改行を加える
                _ = details.AppendLine(new string('-', 40));
                return details.ToString();
            }
            catch (Exception ex)
            {
                _ = System.Windows.MessageBox.Show($"{Name}でエラーが発生しました: {ex.Message}");
                return "";
            }

        }

        public static bool AreGenericTypesCompatible(Type genericTypeA, Type genericTypeB)
        {
            // 両方の型がジェネリック型であるかを確認
            if (!genericTypeA.IsGenericType || !genericTypeB.IsGenericType)
            {
                return false;
            }

            // ジェネリック型定義を比較する
            if (genericTypeA.GetGenericTypeDefinition() != genericTypeB.GetGenericTypeDefinition())
            {
                return false;
            }

            // ジェネリック型パラメータを取得
            Type[] genericArgumentsA = genericTypeA.GetGenericArguments();
            Type[] genericArgumentsB = genericTypeB.GetGenericArguments();

            // 各ジェネリック型パラメータの互換性を確認
            for (int i = 0; i < genericArgumentsA.Length; i++)
            {
                if (!genericArgumentsA[i].IsAssignableFrom(genericArgumentsB[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // ST_BRIDGEクラスをTreeNodeに変換するメソッド
        public static CustomTreeNode? ConvertToTreeNode(object obj, List<IModelElement> modelElements, string nodeName = "")
        {
            if (obj == null)
            {
                return null;
            }

            // CustomTreeNodeを生成し、詳細情報も同時に設定
            CustomTreeNode rootNode;
            if (obj is IModelElement modelElement)
            {
                rootNode = new CustomTreeNode(obj, nodeName == "" ? obj.GetType().Name : nodeName, modelElement);
                modelElements.Add(modelElement);
            }
            else
            {
                rootNode = new(obj, nodeName == "" ? obj.GetType().Name : nodeName);
            }

            // リフレクションでプロパティを取得
            PropertyInfo[] properties = obj.GetType().GetProperties();

            // ElementのみをTreeに表示
            foreach (PropertyInfo prop in properties)
            {
                // 自動で作られるクラスの特性上、プリミティブ型、string型のプロパティはスキップ
                if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                {
                    continue;
                }

                // 自動で作られるクラスのXmlAttribute属性を持つプロパティはスキップ
                if (prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true).Any())
                {
                    continue;
                }

                // partialクラスで追加した特定の型のプロパティをスキップ
                if (AreGenericTypesCompatible(typeof(List<IRender>), prop.PropertyType) || AreGenericTypesCompatible(typeof(List<IPropertyTab>), prop.PropertyType))
                {
                    continue;
                }

                // Items要素を飛ばしてその子要素を表示するロジック
                if (prop.Name == "Items")
                {
                    object? propValue = prop.GetValue(obj, null);
                    if (propValue is System.Collections.IEnumerable)
                    {
                        foreach (object? item in (System.Collections.IEnumerable)propValue)
                        {
                            CustomTreeNode? childNode = ConvertToTreeNode(item, modelElements, item.GetType().Name);
                            if (childNode != null)
                            {
                                rootNode.Children.Add(childNode);
                            }
                        }
                    }
                    continue; // Items自体は表示しないのでスキップ
                }

                try
                {
                    object? propValue = prop.GetValue(obj, null);

                    // nullの場合、またはフィルタ対象の場合はスキップ
                    if (propValue == null)
                    {
                        continue;
                    }
                    else if (propValue is System.Collections.IEnumerable)
                    {
                        // 親ノードはそのまま表示（プロパティ名のみ）
                        CustomTreeNode listNode = new(propValue, prop.Name);

                        foreach (object? item in (System.Collections.IEnumerable)propValue)
                        {
                            // 子要素には代表的なプロパティ（idやnameなど）を取得し、表示名を工夫
                            CustomTreeNode? childNode = ConvertToTreeNode(item, modelElements, GetDisplayNameForItem(item));
                            if (childNode != null)
                            {
                                listNode.Children.Add(childNode);
                            }
                        }

                        if (listNode.Children.Count > 0)
                        {
                            rootNode.Children.Add(listNode);
                        }
                    }
                    else
                    {
                        // 通常のプロパティの場合
                        CustomTreeNode? childNode = ConvertToTreeNode(propValue, modelElements, prop.Name);
                        if (childNode != null)
                        {
                            rootNode.Children.Add(childNode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // エラーログを記録するか、エラーメッセージを適切に表示する
                    Console.WriteLine($"Error accessing property {prop.Name}: {ex.Message}");
                }
            }

            return rootNode;
        }

        // アイテムの表示名を工夫するためのメソッド
        private static string GetDisplayNameForItem(object item)
        {
            if (item == null)
            {
                return item.GetType().Name;
            }

            // リフレクションで主要なプロパティ（idやnameなど）を取得（例: idやname）
            PropertyInfo? idProp = item.GetType().GetProperty("id");
            PropertyInfo? nameProp = item.GetType().GetProperty("name");

            if (idProp != null)
            {
                object? idValue = idProp.GetValue(item);
                if (idValue != null)
                {
                    return $"{item.GetType().Name} (id: {idValue})";
                }
            }
            if (nameProp != null)
            {
                object? nameValue = nameProp.GetValue(item);
                if (nameValue != null)
                {
                    return $"{item.GetType().Name} (name: {nameValue})";
                }
            }

            // 代表的なプロパティが見つからない場合は型名をそのまま返す
            return item.GetType().Name;
        }

    }
}
