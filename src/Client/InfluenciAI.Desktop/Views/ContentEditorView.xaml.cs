using System.Windows.Controls;
using InfluenciAI.Desktop.Services.Content;
using InfluenciAI.Desktop.Services.SocialProfiles;
using InfluenciAI.Desktop.ViewModels;

namespace InfluenciAI.Desktop.Views
{
    public partial class ContentEditorView : UserControl
    {
        public ContentEditorView()
        {
            InitializeComponent();

            var app = (App)System.Windows.Application.Current;
            var contentService = (IContentService)app.Services.GetService(typeof(IContentService))!;
            var socialProfilesService = (ISocialProfilesService)app.Services.GetService(typeof(ISocialProfilesService))!;

            DataContext = new ContentEditorViewModel(contentService, socialProfilesService);
        }
    }
}
