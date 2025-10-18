using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using InfluenciAI.Desktop.Common;
using InfluenciAI.Desktop.Services.Tenants;
using InfluenciAI.Desktop.Services.Users;

namespace InfluenciAI.Desktop.ViewModels;

public sealed class UsersViewModel : INotifyPropertyChanged
{
    private readonly ITenantsService _tenants;
    private readonly IUsersService _users;
    private bool _busy;
    private string _status = string.Empty;
    private TenantDto? _selectedTenant;
    private UserDto? _selectedUser;
    private string _newEmail = string.Empty;
    private string _newPassword = string.Empty;
    private string _newDisplayName = string.Empty;

    public UsersViewModel(ITenantsService tenants, IUsersService users)
    {
        _tenants = tenants;
        _users = users;
        Tenants = new ObservableCollection<TenantDto>();
        Users = new ObservableCollection<UserDto>();

        LoadTenantsCommand = new AsyncCommand(LoadTenantsAsync, () => !Busy);
        LoadUsersCommand = new AsyncCommand(LoadUsersAsync, () => !Busy && SelectedTenant is not null);
        CreateUserCommand = new AsyncCommand(CreateUserAsync, () => !Busy && SelectedTenant is not null && !string.IsNullOrWhiteSpace(NewEmail) && !string.IsNullOrWhiteSpace(NewPassword));
        UpdateUserCommand = new AsyncCommand(UpdateUserAsync, () => !Busy && SelectedTenant is not null && SelectedUser is not null);
        DeleteUserCommand = new AsyncCommand(DeleteUserAsync, () => !Busy && SelectedTenant is not null && SelectedUser is not null);
    }

    public ObservableCollection<TenantDto> Tenants { get; }
    public ObservableCollection<UserDto> Users { get; }

    public TenantDto? SelectedTenant { get => _selectedTenant; set { _selectedTenant = value; OnPropertyChanged(); RaiseCanExecutes(); } }
    public UserDto? SelectedUser { get => _selectedUser; set { _selectedUser = value; OnPropertyChanged(); RaiseCanExecutes(); } }
    public string NewEmail { get => _newEmail; set { _newEmail = value; OnPropertyChanged(); ((AsyncCommand)CreateUserCommand).RaiseCanExecuteChanged(); } }
    public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); ((AsyncCommand)CreateUserCommand).RaiseCanExecuteChanged(); } }
    public string NewDisplayName { get => _newDisplayName; set { _newDisplayName = value; OnPropertyChanged(); } }
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); RaiseCanExecutes(); } }

    public ICommand LoadTenantsCommand { get; }
    public ICommand LoadUsersCommand { get; }
    public ICommand CreateUserCommand { get; }
    public ICommand UpdateUserCommand { get; }
    public ICommand DeleteUserCommand { get; }

    private void RaiseCanExecutes()
    {
        ((AsyncCommand)LoadTenantsCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)LoadUsersCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)CreateUserCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)UpdateUserCommand).RaiseCanExecuteChanged();
        ((AsyncCommand)DeleteUserCommand).RaiseCanExecuteChanged();
    }

    private async Task LoadTenantsAsync()
    {
        Busy = true; Status = string.Empty;
        try
        {
            Tenants.Clear();
            var list = await _tenants.GetAllAsync(CancellationToken.None);
            foreach (var t in list) Tenants.Add(t);
            SelectedTenant ??= Tenants.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally { Busy = false; }
    }

    private async Task LoadUsersAsync()
    {
        if (SelectedTenant is null) return;
        Busy = true; Status = string.Empty;
        try
        {
            Users.Clear();
            var list = await _users.ListAsync(SelectedTenant.Id, CancellationToken.None);
            foreach (var u in list) Users.Add(u);
        }
        catch (Exception ex) { Status = ex.Message; }
        finally { Busy = false; }
    }

    private async Task CreateUserAsync()
    {
        if (SelectedTenant is null) return;
        Busy = true; Status = string.Empty;
        try
        {
            var dto = await _users.CreateAsync(SelectedTenant.Id, NewEmail.Trim(), NewPassword, string.IsNullOrWhiteSpace(NewDisplayName) ? null : NewDisplayName.Trim(), CancellationToken.None);
            Users.Add(dto);
            NewEmail = string.Empty; NewPassword = string.Empty; NewDisplayName = string.Empty;
        }
        catch (Exception ex) { Status = ex.Message; }
        finally { Busy = false; }
    }

    private async Task UpdateUserAsync()
    {
        if (SelectedTenant is null || SelectedUser is null) return;
        Busy = true; Status = string.Empty;
        try
        {
            var dto = await _users.UpdateAsync(SelectedTenant.Id, SelectedUser.Id, SelectedUser.Email, SelectedUser.DisplayName, CancellationToken.None);
            var idx = Users.ToList().FindIndex(u => u.Id == dto.Id);
            if (idx >= 0) { Users[idx] = dto; OnPropertyChanged(nameof(Users)); }
        }
        catch (Exception ex) { Status = ex.Message; }
        finally { Busy = false; }
    }

    private async Task DeleteUserAsync()
    {
        if (SelectedTenant is null || SelectedUser is null) return;
        Busy = true; Status = string.Empty;
        try
        {
            var confirm = MessageBox.Show($"Remover usuário '{SelectedUser.Email}'?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;
            var ok = await _users.DeleteAsync(SelectedTenant.Id, SelectedUser.Id, CancellationToken.None);
            if (ok)
            {
                Users.Remove(SelectedUser);
                SelectedUser = null;
            }
        }
        catch (Exception ex) { Status = ex.Message; }
        finally { Busy = false; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

