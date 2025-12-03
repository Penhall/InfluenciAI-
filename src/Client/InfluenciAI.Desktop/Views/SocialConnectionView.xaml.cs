using System.Windows.Controls;
using InfluenciAI.Desktop.Services.SocialProfiles;
using InfluenciAI.Desktop.ViewModels;

namespace InfluenciAI.Desktop.Views
{
    public partial class SocialConnectionView : UserControl
    {
        public SocialConnectionView()
        {
            InitializeComponent();

            var app = (App)System.Windows.Application.Current;
            var socialProfilesService = (ISocialProfilesService)app.Services.GetService(typeof(ISocialProfilesService))!;

            DataContext = new SocialConnectionViewModel(socialProfilesService);
        }
    }
}
