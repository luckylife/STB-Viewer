using System.Windows;
using System.Windows.Controls;

namespace STBViewer2Lib.DetailsWindow
{
    public interface IPropertyTab
    {
        StackPanel MainStackPanel { get; }
        StackPanel ContentStackPanel { get; }
        string Title { get; }
        StackPanel CreateStackPanel()
        {
            // GroupBoxを作成
            System.Windows.Controls.GroupBox groupBox = new()
            {
                Header = Title,
                Margin = new Thickness(5),
                Content = ContentStackPanel
            };
            _ = MainStackPanel.Children.Add(groupBox);
            return MainStackPanel;
        }
    }


}
