using System.Windows.Controls;
using InfluenciAI.Desktop.Services.Content;
using InfluenciAI.Desktop.Services.Metrics;
using InfluenciAI.Desktop.ViewModels;

namespace InfluenciAI.Desktop.Views
{
    public partial class MetricsView : UserControl
    {
        public MetricsView()
        {
            InitializeComponent();

            var app = (App)System.Windows.Application.Current;
            var contentService = (IContentService)app.Services.GetService(typeof(IContentService))!;
            var metricsService = (IMetricsService)app.Services.GetService(typeof(IMetricsService))!;

            DataContext = new MetricsViewModel(contentService, metricsService);
        }
    }
}
