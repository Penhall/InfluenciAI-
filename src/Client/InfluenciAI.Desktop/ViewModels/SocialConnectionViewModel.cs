using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using InfluenciAI.Desktop.Services.SocialProfiles;

namespace InfluenciAI.Desktop.ViewModels;

public class SocialConnectionViewModel : INotifyPropertyChanged
{
    private readonly ISocialProfilesService _socialProfiles;
    private string _status = string.Empty;
    private bool _busy;

    public SocialConnectionViewModel(ISocialProfilesService socialProfiles)
    {
        _socialProfiles = socialProfiles;
        ConnectedProfiles = new ObservableCollection<SocialProfileDto>();

        ConnectTwitterCommand = new AsyncCommand(ConnectTwitterAsync, () => !Busy);
        RefreshCommand = new AsyncCommand(LoadProfilesAsync, () => !Busy);

        // Load profiles on startup
        _ = LoadProfilesAsync();
    }

    public ObservableCollection<SocialProfileDto> ConnectedProfiles { get; }
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); RaiseCommandsCanExecuteChanged(); } }

    public ICommand ConnectTwitterCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task ConnectTwitterAsync()
    {
        Busy = true;
        Status = "Abrindo navegador para autenticação...";

        try
        {
            var authUrl = await _socialProfiles.GetTwitterAuthUrlAsync(CancellationToken.None);

            // Open browser with OAuth URL
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            Status = "Complete a autorização no navegador e aguarde...";

            // Give user time to authorize, then refresh
            await Task.Delay(TimeSpan.FromSeconds(5));
            await LoadProfilesAsync();

            Status = "Verifique se a conexão foi bem-sucedida acima";
        }
        catch (Exception ex)
        {
            Status = $"Erro: {ex.Message}";
        }
        finally
        {
            Busy = false;
        }
    }

    private async Task LoadProfilesAsync()
    {
        Busy = true;
        Status = "Carregando perfis conectados...";

        try
        {
            var profiles = await _socialProfiles.GetAllAsync(CancellationToken.None);

            ConnectedProfiles.Clear();
            foreach (var profile in profiles)
            {
                ConnectedProfiles.Add(profile);
            }

            Status = profiles.Count > 0
                ? $"{profiles.Count} perfil(is) conectado(s)"
                : "Nenhum perfil conectado";
        }
        catch (Exception ex)
        {
            Status = $"Erro ao carregar perfis: {ex.Message}";
        }
        finally
        {
            Busy = false;
        }
    }

    private void RaiseCommandsCanExecuteChanged()
    {
        ((AsyncCommand)ConnectTwitterCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)RefreshCommand).RaiseCanExecuteChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
