using System.Windows;
using System.Windows.Controls;

namespace SSHHelper.App.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }
}