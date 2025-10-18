using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using InfluenciAI.Desktop.Common;
using InfluenciAI.Desktop.Services.Tenants;
using System.Windows;

namespace InfluenciAI.Desktop.ViewModels;

public sealed class TenantsViewModel : INotifyPropertyChanged
{
    private readonly ITenantsService _service;
    private bool _busy;
    private string _status = string.Empty;
    private TenantDto? _selected;
    private string _newName = string.Empty;

    public TenantsViewModel(ITenantsService service)
    {
        _service = service;
        Tenants = new ObservableCollection<TenantDto>();

        LoadCommand = new AsyncCommand(LoadAsync, () => !Busy);
        CreateCommand = new AsyncCommand(CreateAsync, () => !Busy && !string.IsNullOrWhiteSpace(NewName));
        UpdateCommand = new AsyncCommand(UpdateAsync, () => !Busy && Selected is not null && !string.IsNullOrWhiteSpace(Selected?.Name));
        DeleteCommand = new AsyncCommand(DeleteAsync, () => !Busy && Selected is not null);
    }

    public ObservableCollection<TenantDto> Tenants { get; }
    public TenantDto? Selected { get => _selected; set { _selected = value; OnPropertyChanged(); RaiseCanExecutes(); } }
    public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); ((AsyncCommand)CreateCommand).RaiseCanExecuteChanged(); } }
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); RaiseCanExecutes(); } }

    public ICommand LoadCommand { get; }
    public ICommand CreateCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }

    private void RaiseCanExecutes()
    {
        ((AsyncCommand)LoadCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)CreateCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)UpdateCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)DeleteCommand).RaiseCanExecuteChanged();
    }

    private async Task LoadAsync()
    {
        Busy = true; Status = string.Empty;
        try
        {
            Tenants.Clear();
            var list = await _service.GetAllAsync(CancellationToken.None);
            foreach (var t in list)
                Tenants.Add(t);
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally { Busy = false; }
    }

    private async Task CreateAsync()
    {
        Busy = true; Status = string.Empty;
        try
        {
            var dto = await _service.CreateAsync(NewName.Trim(), CancellationToken.None);
            Tenants.Add(dto);
            NewName = string.Empty;
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally { Busy = false; }
    }

    private async Task UpdateAsync()
    {
        if (Selected is null) return;
        Busy = true; Status = string.Empty;
        try
        {
            var dto = await _service.UpdateAsync(Selected.Id, Selected.Name.Trim(), CancellationToken.None);
            var idx = Tenants.ToList().FindIndex(t => t.Id == dto.Id);
            if (idx >= 0)
            {
                Tenants[idx] = dto;
                OnPropertyChanged(nameof(Tenants));
            }
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally { Busy = false; }
    }

    private async Task DeleteAsync()
    {
        if (Selected is null) return;
        Busy = true; Status = string.Empty;
        try
        {
            var confirm = MessageBox.Show($"Remover tenant '{Selected.Name}'?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) { return; }
            var ok = await _service.DeleteAsync(Selected.Id, CancellationToken.None);
            if (ok)
            {
                Tenants.Remove(Selected);
                Selected = null;
            }
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally { Busy = false; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
