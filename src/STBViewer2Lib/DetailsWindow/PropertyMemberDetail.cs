namespace STBViewer2Lib.DetailsWindow
{
    // プロパティ詳細クラス
    public class PropertyMemberDetail
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public List<string> Children { get; set; }

        public PropertyMemberDetail(string name, string value, List<string>? children = null)
        {
            PropertyName = name;
            PropertyValue = value;
            Children = children;
        }
    }
}
