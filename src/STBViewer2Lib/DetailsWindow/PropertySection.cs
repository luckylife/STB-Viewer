using System.Windows;
using System.Windows.Controls;

namespace STBViewer2Lib.DetailsWindow
{
    public class PropertySection : IPropertyTab
    {
        public StackPanel MainStackPanel { get; } = new();

        public StackPanel ContentStackPanel { get; private set; } = new();

        public string Title { get; }

        public PropertySection(string title, List<PropertyDetail> propertyDetails)
        {
            Title = title;
            foreach (PropertyDetail detail in propertyDetails)
            {
                foreach (TextBlock textBlock in CreateTextBlocks(detail, true, 0))
                {
                    _ = ContentStackPanel.Children.Add(textBlock);
                }
            }
        }

        // テキストブロックを作成するヘルパーメソッド
        private List<TextBlock> CreateTextBlocks(PropertyDetail detail, bool isBold, int indent)
        {
            List<TextBlock> textBlocks = [CreateTextBlock(detail, isBold, 10 * (indent + 1))];
            if (detail.Children != null && detail.Children.Count > 0)
            {
                foreach (PropertyDetail child in detail.Children)
                {
                    textBlocks.AddRange(CreateTextBlocks(child, false, indent + 1));
                }
            }
            return textBlocks;
        }

        private TextBlock CreateTextBlock(PropertyDetail detail, bool isBold, int margin)
        {
            return new TextBlock
            {
                Text = $"{detail.PropertyName}: {detail.PropertyValue}",
                Margin = new Thickness(margin, 0, 0, 0),
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isBold ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Gray
            };
        }
    }
}
