using System.Windows;
using InfluenciAI.Desktop.ViewModels;

namespace InfluenciAI.Desktop.Views;

public partial class UsersView : System.Windows.Controls.UserControl
{
    public UsersView()
    {
        InitializeComponent();
        var app = (App)System.Windows.Application.Current;
        var tenantsSvc = (Services.Tenants.ITenantsService)app.Services.GetService(typeof(Services.Tenants.ITenantsService))!;
        var usersSvc = (Services.Users.IUsersService)app.Services.GetService(typeof(Services.Users.IUsersService))!;
        DataContext = new UsersViewModel(tenantsSvc, usersSvc);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is UsersViewModel vm)
        {
            vm.LoadTenantsCommand.Execute(null);
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is UsersViewModel vm)
        {
            vm.NewPassword = (this.FindName("Pwd") as System.Windows.Controls.PasswordBox)?.Password ?? string.Empty;
            vm.CreateUserCommand.Execute(null);
        }
    }
}

