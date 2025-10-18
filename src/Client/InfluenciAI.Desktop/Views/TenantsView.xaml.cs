using System.Windows;
using InfluenciAI.Desktop.ViewModels;

namespace InfluenciAI.Desktop.Views;

public partial class TenantsView : System.Windows.Controls.UserControl
{
    public TenantsView()
    {
        InitializeComponent();
        var app = (App)System.Windows.Application.Current;
        var svc = (Services.Tenants.ITenantsService)app.Services.GetService(typeof(Services.Tenants.ITenantsService))!;
        DataContext = new TenantsViewModel(svc);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TenantsViewModel vm)
        {
            // Fire-and-forget initial load
            vm.LoadCommand.Execute(null);
        }
    }
}
