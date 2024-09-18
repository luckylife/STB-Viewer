namespace STBViewer2Lib.DetailsWindow
{

    public class PropertyDetail
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public List<PropertyDetail> Children { get; set; }

        public PropertyDetail(string name, string value, List<PropertyDetail>? children = null)
        {
            PropertyName = name;
            PropertyValue = value;
            Children = children;
        }
    }
}
