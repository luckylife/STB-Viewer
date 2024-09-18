using System.Windows;
using System.Windows.Controls;

namespace STBViewer2Lib.DetailsWindow
{
    public class PropertyTabMember : IPropertyTab
    {
        public StackPanel MainStackPanel { get; } = new();

        public StackPanel ContentStackPanel { get; private set; } = new();

        public string Title { get; }

        public PropertyTabMember(string title, List<PropertyMemberDetail> propertyDetails)
        {
            Title = title;
            foreach (PropertyMemberDetail detail in propertyDetails)
            {
                foreach (TextBlock textBlock in CreateTextBlocks(detail, true, 0))
                {
                    _ = ContentStackPanel.Children.Add(textBlock);
                }
            }
        }

        // テキストブロックを作成するヘルパーメソッド
        private List<TextBlock> CreateTextBlocks(PropertyMemberDetail detail, bool isBold, int indent)
        {
            List<TextBlock> textBlocks = [CreateTextBlock(detail, isBold, 5 * (indent + 1))];
            if (detail.Children != null && detail.Children.Count > 0)
            {
                foreach (string child in detail.Children)
                {
                    textBlocks.Add(CreateTextBlock(child, false, 5 * (indent + 2)));
                }
            }
            return textBlocks;
        }

        private TextBlock CreateTextBlock(PropertyMemberDetail detail, bool isBold, int margin)
        {
            return new TextBlock
            {
                Text = $"{detail.PropertyName}: {detail.PropertyValue}",
                Margin = new Thickness(margin, 0, 0, 0),
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isBold ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Gray
            };
        }

        private TextBlock CreateTextBlock(string detail, bool isBold, int margin)
        {
            return new TextBlock
            {
                Text = detail,
                Margin = new Thickness(margin, 0, 0, 0),
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isBold ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Gray
            };
        }
    }
}
