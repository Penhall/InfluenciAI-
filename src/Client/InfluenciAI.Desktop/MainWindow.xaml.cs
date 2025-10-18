using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InfluenciAI.Desktop.Common;

namespace InfluenciAI.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string _currentUserLabel = string.Empty;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        var app = (App)System.Windows.Application.Current;
        var state = (Services.Auth.IAuthTokenProvider)app.Services.GetService(typeof(Services.Auth.IAuthTokenProvider))!;
        var (name, tenant, roles) = JwtUtils.Parse(state.AccessToken);
        CurrentUserLabel = name is null ? string.Empty : $"Logado como: {name}{(tenant is null ? string.Empty : $" | Tenant: {tenant}")}";
    }
    public string CurrentUserLabel { get => _currentUserLabel; set { _currentUserLabel = value; OnPropertyChanged(); } }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)System.Windows.Application.Current;
        var auth = (Services.Auth.IAuthService)app.Services.GetService(typeof(Services.Auth.IAuthService))!;
        var state = (Services.Auth.IAuthTokenProvider)app.Services.GetService(typeof(Services.Auth.IAuthTokenProvider))!;
        try
        {
            await auth.LogoutAsync(System.Threading.CancellationToken.None);
        }
        catch { /* ignore */ }
        finally
        {
            if (state is Services.Auth.AuthState s) { s.SetTokens(string.Empty, string.Empty); }
            var login = new Views.LoginView();
            login.Show();
            this.Close();
        }
    }

    private async void LogoutAll_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)System.Windows.Application.Current;
        var auth = (Services.Auth.IAuthService)app.Services.GetService(typeof(Services.Auth.IAuthService))!;
        var state = (Services.Auth.IAuthTokenProvider)app.Services.GetService(typeof(Services.Auth.IAuthTokenProvider))!;
        try
        {
            await auth.LogoutAllAsync(System.Threading.CancellationToken.None);
        }
        catch { /* ignore */ }
        finally
        {
            if (state is Services.Auth.AuthState s) { s.SetTokens(string.Empty, string.Empty); }
            var login = new Views.LoginView();
            login.Show();
            this.Close();
        }
    }
}

public partial class MainWindow
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
