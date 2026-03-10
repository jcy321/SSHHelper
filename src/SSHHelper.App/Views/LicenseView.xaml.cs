using System.Windows;

namespace SSHHelper.App.Views;

public partial class LicenseView : Window
{
    public LicenseView()
    {
        InitializeComponent();
        
        // 允许拖动窗口
        MouseLeftButtonDown += (s, e) => DragMove();
    }
}