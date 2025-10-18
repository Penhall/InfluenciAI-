using System;
using System.Windows;
using InfluenciAI.Desktop.ViewModels;

namespace InfluenciAI.Desktop.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        var app = (App)System.Windows.Application.Current;
        DataContext = new LoginViewModel(app.Services.GetService(typeof(Services.Auth.IAuthService)) as Services.Auth.IAuthService ?? throw new InvalidOperationException("AuthService not registered"), OnLoginSuccess);
    }

    private void OnLoginSuccess()
    {
        var main = new MainWindow();
        main.Show();
        this.Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        // Bridge PasswordBox to ViewModel
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = (this.FindName("PwdBox") as System.Windows.Controls.PasswordBox)?.Password ?? string.Empty;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }
}



