using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using InfluenciAI.Desktop.Services.Auth;

namespace InfluenciAI.Desktop.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _auth;
    private readonly Action _onSuccess;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _status = string.Empty;
    private bool _busy;

    public LoginViewModel(IAuthService auth, Action onSuccess)
    {
        _auth = auth;
        _onSuccess = onSuccess;
        LoginCommand = new AsyncCommand(DoLoginAsync, () => !Busy);
    }

    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
    public bool Busy { get => _busy; set { _busy = value; OnPropertyChanged(); ((AsyncCommand)LoginCommand).RaiseCanExecuteChanged(); } }

    public ICommand LoginCommand { get; }

    private async Task DoLoginAsync()
    {
        Busy = true; Status = string.Empty;
        try
        {
            var ok = await _auth.LoginAsync(Email, Password, CancellationToken.None);
            if (ok) _onSuccess(); else Status = "Falha no login";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally { Busy = false; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) { _execute = execute; _canExecute = canExecute; }
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public async void Execute(object? parameter) => await _execute();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

