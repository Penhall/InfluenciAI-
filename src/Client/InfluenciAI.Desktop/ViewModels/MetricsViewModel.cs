using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using InfluenciAI.Desktop.Services.Content;
using InfluenciAI.Desktop.Services.Metrics;

namespace InfluenciAI.Desktop.ViewModels;

public class MetricsViewModel : INotifyPropertyChanged
{
    private readonly IContentService _contentService;
    private readonly IMetricsService _metricsService;
    private readonly DispatcherTimer _refreshTimer;
    private ContentDto? _selectedContent;
    private ContentMetricsDto? _currentMetrics;
    private string _status = string.Empty;
    private bool _busy;
    private bool _autoRefreshEnabled = true;

    public MetricsViewModel(IContentService contentService, IMetricsService metricsService)
    {
        _contentService = contentService;
        _metricsService = metricsService;

        PublishedContents = new ObservableCollection<ContentDto>();
        MetricsTimeseries = new ObservableCollection<MetricSnapshotDto>();

        RefreshCommand = new AsyncCommand(LoadMetricsAsync, () => !Busy);
        LoadContentsCommand = new AsyncCommand(LoadPublishedContentsAsync, () => !Busy);

        // Setup auto-refresh timer (every 30 seconds)
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _refreshTimer.Tick += async (s, e) =>
        {
            if (AutoRefreshEnabled && SelectedContent != null)
            {
                await LoadMetricsAsync();
            }
        };

        // Load published contents on startup
        _ = LoadPublishedContentsAsync();
    }

    public ObservableCollection<ContentDto> PublishedContents { get; }
    public ObservableCollection<MetricSnapshotDto> MetricsTimeseries { get; }

    public ContentDto? SelectedContent
    {
        get => _selectedContent;
        set
        {
            _selectedContent = value;
            OnPropertyChanged();
            _ = LoadMetricsAsync();
        }
    }

    public ContentMetricsDto? CurrentMetrics
    {
        get => _currentMetrics;
        set
        {
            _currentMetrics = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LatestViews));
            OnPropertyChanged(nameof(LatestLikes));
            OnPropertyChanged(nameof(LatestRetweets));
            OnPropertyChanged(nameof(LatestReplies));
            OnPropertyChanged(nameof(LatestEngagementRate));
        }
    }

    public bool AutoRefreshEnabled
    {
        get => _autoRefreshEnabled;
        set
        {
            _autoRefreshEnabled = value;
            OnPropertyChanged();

            if (value)
                _refreshTimer.Start();
            else
                _refreshTimer.Stop();
        }
    }

    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); RaiseCommandsCanExecuteChanged(); } }

    // Convenience properties for binding
    public int LatestViews => CurrentMetrics?.Latest?.Views ?? 0;
    public int LatestLikes => CurrentMetrics?.Latest?.Likes ?? 0;
    public int LatestRetweets => CurrentMetrics?.Latest?.Retweets ?? 0;
    public int LatestReplies => CurrentMetrics?.Latest?.Replies ?? 0;
    public string LatestEngagementRate => CurrentMetrics?.Latest?.EngagementRate.ToString("P2") ?? "0%";

    public ICommand RefreshCommand { get; }
    public ICommand LoadContentsCommand { get; }

    private async Task LoadPublishedContentsAsync()
    {
        Busy = true;
        Status = "Carregando conteúdos publicados...";

        try
        {
            var contents = await _contentService.GetAllAsync(CancellationToken.None);

            PublishedContents.Clear();
            foreach (var content in contents.Where(c => c.Status == Domain.Entities.ContentStatus.Published))
            {
                PublishedContents.Add(content);
            }

            // Auto-select first if available
            if (PublishedContents.Count > 0 && SelectedContent == null)
            {
                SelectedContent = PublishedContents[0];
            }

            Status = PublishedContents.Count > 0
                ? $"{PublishedContents.Count} conteúdo(s) publicado(s)"
                : "Nenhum conteúdo publicado";
        }
        catch (Exception ex)
        {
            Status = $"Erro ao carregar conteúdos: {ex.Message}";
        }
        finally
        {
            Busy = false;
        }
    }

    private async Task LoadMetricsAsync()
    {
        if (SelectedContent == null) return;

        Busy = true;
        Status = "Carregando métricas...";

        try
        {
            var metrics = await _metricsService.GetContentMetricsAsync(SelectedContent.Id, CancellationToken.None);

            if (metrics != null)
            {
                CurrentMetrics = metrics;

                // Update timeseries chart data
                MetricsTimeseries.Clear();
                if (metrics.Timeseries != null)
                {
                    foreach (var snapshot in metrics.Timeseries.OrderBy(s => s.CollectedAt))
                    {
                        MetricsTimeseries.Add(snapshot);
                    }
                }

                var timeSincePublish = DateTime.UtcNow - (metrics.PublishedAt ?? DateTime.UtcNow);
                Status = $"Métricas atualizadas (publicado há {timeSincePublish.TotalHours:F1}h)";
            }
            else
            {
                Status = "Nenhuma métrica disponível ainda";
            }
        }
        catch (Exception ex)
        {
            Status = $"Erro ao carregar métricas: {ex.Message}";
        }
        finally
        {
            Busy = false;
        }
    }

    private void RaiseCommandsCanExecuteChanged()
    {
        ((AsyncCommand)RefreshCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)LoadContentsCommand).RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
