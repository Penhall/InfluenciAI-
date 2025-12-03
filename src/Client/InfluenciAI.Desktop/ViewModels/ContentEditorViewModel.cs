using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using InfluenciAI.Desktop.Services.Content;
using InfluenciAI.Desktop.Services.SocialProfiles;
using InfluenciAI.Domain.Entities;

namespace InfluenciAI.Desktop.ViewModels;

public class ContentEditorViewModel : INotifyPropertyChanged
{
    private readonly IContentService _contentService;
    private readonly ISocialProfilesService _socialProfilesService;
    private string _tweetText = string.Empty;
    private string _status = string.Empty;
    private bool _busy;
    private SocialProfileDto? _selectedProfile;

    public ContentEditorViewModel(IContentService contentService, ISocialProfilesService socialProfilesService)
    {
        _contentService = contentService;
        _socialProfilesService = socialProfilesService;

        ConnectedProfiles = new ObservableCollection<SocialProfileDto>();
        PublishCommand = new AsyncCommand(PublishAsync, CanPublish);

        // Load profiles on startup
        _ = LoadProfilesAsync();
    }

    public ObservableCollection<SocialProfileDto> ConnectedProfiles { get; }

    public string TweetText
    {
        get => _tweetText;
        set
        {
            _tweetText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CharacterCount));
            OnPropertyChanged(nameof(CharactersRemaining));
            OnPropertyChanged(nameof(IsOverLimit));
            ((AsyncCommand)PublishCommand).RaiseCanExecuteChanged();
        }
    }

    public SocialProfileDto? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            _selectedProfile = value;
            OnPropertyChanged();
            ((AsyncCommand)PublishCommand).RaiseCanExecuteChanged();
        }
    }

    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); ((AsyncCommand)PublishCommand).RaiseCanExecuteChanged(); } }

    public int CharacterCount => TweetText.Length;
    public int CharactersRemaining => 280 - CharacterCount;
    public bool IsOverLimit => CharacterCount > 280;

    public ICommand PublishCommand { get; }

    private bool CanPublish()
    {
        return !Busy
            && !string.IsNullOrWhiteSpace(TweetText)
            && !IsOverLimit
            && SelectedProfile != null;
    }

    private async Task PublishAsync()
    {
        if (SelectedProfile == null) return;

        Busy = true;
        Status = "Criando conteúdo...";

        try
        {
            // Create content
            var content = await _contentService.CreateAsync(
                new CreateContentRequest("Tweet", TweetText, ContentType.Text),
                CancellationToken.None
            );

            Status = "Publicando no Twitter...";

            // Publish to Twitter
            var publication = await _contentService.PublishAsync(
                content.Id,
                SelectedProfile.Id,
                CancellationToken.None
            );

            if (publication.Status == PublicationStatus.Published)
            {
                Status = $"✓ Publicado com sucesso! URL: {publication.ExternalUrl}";
                TweetText = string.Empty; // Clear after success
            }
            else
            {
                Status = $"✗ Falha ao publicar: {publication.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            Status = $"✗ Erro: {ex.Message}";
        }
        finally
        {
            Busy = false;
        }
    }

    private async Task LoadProfilesAsync()
    {
        try
        {
            var profiles = await _socialProfilesService.GetAllAsync(CancellationToken.None);

            ConnectedProfiles.Clear();
            foreach (var profile in profiles.Where(p => p.IsActive))
            {
                ConnectedProfiles.Add(profile);
            }

            // Auto-select first profile if available
            if (ConnectedProfiles.Count > 0 && SelectedProfile == null)
            {
                SelectedProfile = ConnectedProfiles[0];
            }

            if (ConnectedProfiles.Count == 0)
            {
                Status = "Nenhum perfil conectado. Conecte uma conta primeiro.";
            }
        }
        catch (Exception ex)
        {
            Status = $"Erro ao carregar perfis: {ex.Message}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
