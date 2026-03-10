using System.Windows;

namespace SSHHelper.App.Views;

public partial class LicenseView : Window
{
    public LicenseView()
    {
        InitializeComponent();
        
        MouseLeftButtonDown += (s, e) => DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
        Application.Current.Shutdown();
    }
}