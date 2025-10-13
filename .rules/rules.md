# Regras de Desenvolvimento - InfluenciAI WPF/C#

**Versão:** 1.0  
**Última atualização:** 13/10/2025  
**Stack:** .NET 8 + WPF + Entity Framework Core + PostgreSQL

---

## 📑 Índice

1. [Linguagem e Documentação](#1-linguagem-e-documentação)
2. [Encoding e Line Endings](#2-encoding-e-line-endings)
3. [Estrutura de Arquivos](#3-estrutura-de-arquivos)
4. [Padrões de Código C#](#4-padrões-de-código-c)
5. [Padrões XAML](#5-padrões-xaml)
6. [Gestão de Ícones](#6-gestão-de-ícones)
7. [MVVM Pattern](#7-mvvm-pattern)
8. [Injeção de Dependências](#8-injeção-de-dependências)
9. [Tratamento de Erros](#9-tratamento-de-erros)
10. [Testes](#10-testes)
11. [Performance](#11-performance)
12. [Segurança](#12-segurança)
13. [Checklist de Código](#13-checklist-de-código)

---

## 1. Linguagem e Documentação

### Código
**Inglês obrigatório** para:
- Namespaces, classes, interfaces, enums
- Variáveis, métodos, propriedades
- Comentários técnicos
- Commits, branches, PRs

```csharp
// ✅ CORRETO
public class AnalysisService : IAnalysisService
{
    public async Task<AnalysisResult> AnalyzePostAsync(string postUrl)
    {
        // ...
    }
}

// ❌ ERRADO
public class ServicoAnalise : IServicoAnalise
{
    public async Task<ResultadoAnalise> AnalisarPostAsync(string urlPost)
    {
        // ...
    }
}
```

### Documentação
**Português (BR)** para:
- `README.md`, `CHANGELOG.md`
- Documentação de negócio (PRD, blueprints)
- Comentários explicativos de alto nível (XML docs podem ser em PT-BR)
- Issues/tasks de gestão

```csharp
/// <summary>
/// Calcula a taxa de engajamento de um post do Twitter/X.
/// </summary>
/// <param name="metrics">Métricas públicas do tweet</param>
/// <returns>Taxa de engajamento entre 0.0 e 1.0</returns>
/// <remarks>
/// Fórmula: (likes + retweets + replies + quotes) / impressions
/// </remarks>
public double CalculateEngagement(PublicMetrics metrics)
{
    // ...
}
```

---

## 2. Encoding e Line Endings

### .editorconfig Obrigatório

```ini
# .editorconfig (raiz da solução)
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

# C# files
[*.cs]
charset = utf-8-bom
indent_style = space
indent_size = 4
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# XAML files
[*.xaml]
charset = utf-8-bom
indent_style = space
indent_size = 4

# JSON, XML
[*.{json,xml}]
indent_size = 2

# Scripts
[*.{ps1,sh}]
end_of_line = crlf
```

### Verificação Pré-Commit

```powershell
# scripts/Check-Encoding.ps1
$files = git diff --cached --name-only --diff-filter=ACM | Where-Object { $_ -match '\.(cs|xaml)$' }

foreach ($file in $files) {
    $content = Get-Content $file -Raw -Encoding UTF8
    
    # Detectar mojibake comum
    if ($content -match 'Ã[ƒ‚¯]|Ã¢[Å"ï¿½]|Ã°Å¸') {
        Write-Error "❌ Mojibake detectado em $file"
        Write-Error "Execute: scripts/Repair-Mojibake.ps1 $file"
        exit 1
    }
}

Write-Host "✅ Encoding validado"
```

### Regra Crítica: UTF-8 com BOM
- **C# e XAML**: Sempre `UTF-8 with BOM`
- **JSON, MD**: `UTF-8 without BOM`
- Visual Studio já configura corretamente por padrão

---

## 3. Estrutura de Arquivos

### Solução Completa

```
InfluenciAI.sln
│
├── src/
│   ├── 1-Core/
│   │   ├── InfluenciAI.Domain/                # Entidades, Value Objects
│   │   ├── InfluenciAI.Application/           # Use Cases, DTOs
│   │   └── InfluenciAI.Contracts/             # Interfaces compartilhadas
│   │
│   ├── 2-Infrastructure/
│   │   ├── InfluenciAI.Infrastructure.Data/   # EF Core, Repositórios
│   │   ├── InfluenciAI.Infrastructure.Cache/  # Redis
│   │   ├── InfluenciAI.Infrastructure.Identity/ # ASP.NET Identity
│   │   └── InfluenciAI.Infrastructure.External/ # Twitter API, OpenAI
│   │
│   ├── 3-Services/
│   │   ├── InfluenciAI.Api/                   # ASP.NET Core Web API
│   │   └── InfluenciAI.Service.Analysis/      # Workers (opcional)
│   │
│   ├── 4-Presentation/
│   │   ├── InfluenciAI.Desktop.WPF/           # Cliente Desktop
│   │   │   ├── Views/
│   │   │   ├── ViewModels/
│   │   │   ├── Services/
│   │   │   ├── Controls/
│   │   │   ├── Converters/
│   │   │   └── Resources/
│   │   │       ├── Themes/
│   │   │       ├── Styles/
│   │   │       └── Icons/
│   │   └── InfluenciAI.Desktop.Core/          # Lógica compartilhada
│   │
│   └── 5-CrossCutting/
│       ├── InfluenciAI.Common/                # Utilitários
│       └── InfluenciAI.Logging/               # Serilog
│
├── tests/
│   ├── InfluenciAI.Domain.Tests/
│   ├── InfluenciAI.Application.Tests/
│   ├── InfluenciAI.Infrastructure.Tests/
│   ├── InfluenciAI.Api.Tests/
│   └── InfluenciAI.Desktop.Tests/
│
├── tools/
│   ├── DataMigration/
│   └── Scripts/
│
└── docs/
    ├── Architecture/
    ├── API/
    └── UserGuides/
```

### Organização de Views WPF

```
Views/
├── Auth/
│   ├── LoginView.xaml
│   └── RegisterView.xaml
├── Dashboard/
│   ├── DashboardView.xaml
│   └── OverviewView.xaml
├── Analysis/
│   ├── AnalysisView.xaml
│   ├── MetricsView.xaml
│   └── RecommendationsView.xaml
├── History/
│   └── HistoryView.xaml
├── Settings/
│   └── SettingsView.xaml
└── Shared/
    ├── LoadingView.xaml
    └── ErrorView.xaml
```

---

## 4. Padrões de Código C#

### Nullable Reference Types

```csharp
// Sempre habilitado no .csproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

```csharp
// ✅ Uso correto
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;  // Não-nulo
    public string? Name { get; set; }                   // Nullable
    public DateTime CreatedAt { get; set; }
}

// ✅ Parâmetros nullable
public async Task<Analysis?> GetAnalysisAsync(Guid id)
{
    return await _context.Analyses.FindAsync(id); // Pode retornar null
}
```

### Async/Await

```csharp
// ✅ CORRETO - Async todo o caminho
public async Task<AnalysisResult> AnalyzePostAsync(string url)
{
    var tweet = await _twitterClient.GetTweetAsync(postId);
    var metrics = await CalculateMetricsAsync(tweet);
    await _cache.SetAsync(key, metrics);
    return new AnalysisResult(metrics);
}

// ❌ ERRADO - Bloqueio com .Result
public AnalysisResult AnalyzePost(string url)
{
    var tweet = _twitterClient.GetTweetAsync(postId).Result; // Deadlock!
    return ProcessTweet(tweet);
}

// ✅ Cancellation Token sempre presente
public async Task<List<Analysis>> GetUserAnalysesAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
{
    return await _context.Analyses
        .Where(a => a.UserId == userId)
        .ToListAsync(cancellationToken);
}
```

### Naming Conventions

```csharp
// ✅ Classes, Métodos, Propriedades: PascalCase
public class AnalysisService : IAnalysisService
{
    public string ServiceName { get; set; }
    
    public async Task ProcessAnalysisAsync() { }
}

// ✅ Campos privados: _camelCase
private readonly ITwitterClient _twitterClient;
private readonly ICacheService _cache;
private int _retryCount;

// ✅ Parâmetros, variáveis locais: camelCase
public void CalculateMetrics(PublicMetrics metrics)
{
    int totalEngagement = metrics.Likes + metrics.Retweets;
    double engagementRate = CalculateRate(totalEngagement);
}

// ✅ Interfaces: I + PascalCase
public interface IAnalysisService { }
public interface ICacheService { }

// ✅ Constantes: PascalCase ou UPPER_CASE
public const string DefaultCacheKey = "analysis";
public const int MAX_RETRY_COUNT = 3;
```

### Records para DTOs

```csharp
// ✅ PREFERIR records para DTOs imutáveis
public record AnalysisRequest(string Url, string Type);

public record AnalysisResponse(
    Guid Id,
    string PostId,
    Dictionary<string, object> Metrics,
    string? Insights,
    List<string> Recommendations
);

// ✅ Record com validação
public record CreateUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? Name { get; init; }
}
```

### Pattern Matching

```csharp
// ✅ Usar pattern matching moderno
public string GetStatusMessage(AnalysisStatus status) => status switch
{
    AnalysisStatus.Pending => "Aguardando processamento",
    AnalysisStatus.Processing => "Processando análise",
    AnalysisStatus.Completed => "Análise concluída",
    AnalysisStatus.Failed => "Falha na análise",
    _ => throw new ArgumentOutOfRangeException(nameof(status))
};

// ✅ Null checks com pattern matching
if (analysis is { Status: AnalysisStatus.Completed, Metrics: not null })
{
    DisplayMetrics(analysis.Metrics);
}
```

### LINQ Legível

```csharp
// ✅ Query syntax para queries complexas
var recentAnalyses = from analysis in _context.Analyses
                     where analysis.UserId == userId
                     where analysis.CreatedAt >= DateTime.UtcNow.AddDays(-7)
                     orderby analysis.CreatedAt descending
                     select new AnalysisDto
                     {
                         Id = analysis.Id,
                         PostId = analysis.PostId,
                         CreatedAt = analysis.CreatedAt
                     };

// ✅ Method syntax para queries simples
var activeUsers = _context.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.Name)
    .ToListAsync();
```

---

## 5. Padrões XAML

### Estrutura de View

```xaml
<UserControl x:Class="InfluenciAI.Desktop.WPF.Views.Analysis.AnalysisView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:InfluenciAI.Desktop.WPF.ViewModels"
             xmlns:fluent="clr-namespace:FluentIcons.WPF;assembly=FluentIcons.WPF"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:AnalysisViewModel, IsDesignTimeCreatable=True}"
             d:DesignHeight="600" d:DesignWidth="800">
    
    <!-- Resources locais -->
    <UserControl.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis"/>
    </UserControl.Resources>
    
    <!-- Conteúdo principal -->
    <Grid>
        <!-- Layout -->
    </Grid>
</UserControl>
```

### Binding Best Practices

```xaml
<!-- ✅ Binding com Mode explícito -->
<TextBox Text="{Binding PostUrl, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
         Style="{StaticResource ModernTextBoxStyle}"/>

<!-- ✅ Binding com fallback -->
<TextBlock Text="{Binding AnalysisResult.Metrics.Likes, FallbackValue='--'}"
           FontSize="14"/>

<!-- ✅ Binding com StringFormat -->
<TextBlock Text="{Binding CreatedAt, StringFormat='dd/MM/yyyy HH:mm'}"
           FontSize="11"/>

<!-- ✅ Command com CommandParameter -->
<Button Content="Analisar"
        Command="{Binding AnalyzeCommand}"
        CommandParameter="{Binding SelectedAnalysisType}"
        Style="{StaticResource PrimaryButtonStyle}"/>
```

### Resources e Estilos

```xaml
<!-- ✅ StaticResource para performance -->
<Button Background="{StaticResource PrimaryBrush}"
        Style="{StaticResource ModernButtonStyle}"/>

<!-- ✅ DynamicResource apenas quando necessário -->
<TextBlock Foreground="{DynamicResource ThemeForegroundBrush}"/>

<!-- ✅ BasedOn para herança de estilos -->
<Style x:Key="PrimaryButton" TargetType="Button" BasedOn="{StaticResource ModernButtonStyle}">
    <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
    <Setter Property="MinWidth" Value="120"/>
</Style>
```

### x:Name vs x:Key

```xaml
<!-- ✅ x:Name para elementos acessíveis no code-behind -->
<TextBox x:Name="PostUrlTextBox"
         Text="{Binding PostUrl}"/>

<!-- ✅ x:Key para recursos -->
<SolidColorBrush x:Key="PrimaryBrush" Color="#5E81AC"/>
<Style x:Key="HeaderStyle" TargetType="TextBlock">
    <Setter Property="FontSize" Value="16"/>
</Style>
```

### DataTemplates

```xaml
<!-- ✅ DataTemplate com x:Key -->
<DataTemplate x:Key="AnalysisItemTemplate" DataType="{x:Type models:Analysis}">
    <Border Style="{StaticResource CardBorderStyle}">
        <StackPanel>
            <TextBlock Text="{Binding PostId}" FontWeight="Bold"/>
            <TextBlock Text="{Binding CreatedAt, StringFormat='dd/MM/yyyy'}"/>
        </StackPanel>
    </Border>
</DataTemplate>

<!-- ✅ ItemsControl com template -->
<ItemsControl ItemsSource="{Binding RecentAnalyses}"
              ItemTemplate="{StaticResource AnalysisItemTemplate}"/>
```

---

## 6. Gestão de Ícones

### Regra Crítica
**NUNCA use emojis diretamente em código C#!**

### FluentIcons (Recomendado)

```xml
<!-- Instalar pacote -->
<PackageReference Include="FluentIcons.WPF" Version="1.1.251" />
```

```xaml
<!-- ✅ Uso direto -->
<fluent:SymbolIcon Symbol="Target" FontSize="16" Foreground="White"/>

<!-- ✅ Em Button -->
<Button Style="{StaticResource IconButtonStyle}">
    <fluent:SymbolIcon Symbol="ChartMultiple" FontSize="14"/>
</Button>

<!-- ✅ Com texto -->
<StackPanel Orientation="Horizontal">
    <fluent:SymbolIcon Symbol="Info" FontSize="12" Margin="0,0,5,0"/>
    <TextBlock Text="Informação" VerticalAlignment="Center"/>
</StackPanel>
```

### Mapeamento via Converter

```csharp
// ✅ Converter para strings → Symbols
public class IconStringToSymbolConverter : IValueConverter
{
    private static readonly Dictionary<string, Symbol> IconMap = new()
    {
        ["TARGET"] = Symbol.Target,
        ["CHART"] = Symbol.ChartMultiple,
        ["STATS"] = Symbol.DataBarVertical,
        ["MODELS"] = Symbol.AppFolder,
        ["VALIDATE"] = Symbol.CheckmarkCircle,
        ["COMPARE"] = Symbol.ArrowCompare,
        ["SETTINGS"] = Symbol.Settings,
        ["INFO"] = Symbol.Info,
        ["WARNING"] = Symbol.Warning,
        ["ERROR"] = Symbol.ErrorCircle,
        ["SUCCESS"] = Symbol.CheckmarkCircle
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string iconString && IconMap.TryGetValue(iconString.ToUpperInvariant(), out var symbol))
            return symbol;
        
        return Symbol.Info; // Default
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

### ASCII Tags em C# (Fallback)

```csharp
// ✅ Tags ASCII para logs/status
public static class StatusTags
{
    public const string OK = "[OK]";
    public const string ERROR = "[ERROR]";
    public const string WARN = "[WARN]";
    public const string INFO = "[INFO]";
    public const string WAIT = "[WAIT]";
}

// ✅ Uso em logs
_logger.LogInformation("{Tag} Análise iniciada para post {PostId}", StatusTags.INFO, postId);
_logger.LogError("{Tag} Falha ao processar: {Error}", StatusTags.ERROR, ex.Message);
```

### Emojis apenas em XAML

```xaml
<!-- ✅ PERMITIDO - XAML é UTF-8 nativo -->
<TextBlock Text="✅ Operação concluída" FontSize="12"/>
<TextBlock Text="⏳ Processando..." FontSize="12"/>

<!-- ❌ PROIBIDO - Code-behind -->
<!-- StatusText.Text = "✅ Sucesso"; // NÃO FAZER! -->
```

---

## 7. MVVM Pattern

### ViewModel Base

```csharp
// ✅ Usar CommunityToolkit.Mvvm
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class AnalysisViewModel : ObservableObject
{
    private readonly IAnalysisService _analysisService;
    private readonly IDialogService _dialogService;

    // ✅ [ObservableProperty] gera propriedade automaticamente
    [ObservableProperty]
    private string _postUrl = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private AnalysisResult? _currentAnalysis;

    public AnalysisViewModel(
        IAnalysisService analysisService,
        IDialogService dialogService)
    {
        _analysisService = analysisService;
        _dialogService = dialogService;
    }

    // ✅ [RelayCommand] gera comando automaticamente
    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        IsProcessing = true;
        
        try
        {
            CurrentAnalysis = await _analysisService.AnalyzePostAsync(PostUrl);
            await _dialogService.ShowSuccessAsync("Análise concluída!");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync($"Erro: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private bool CanAnalyze() => !string.IsNullOrWhiteSpace(PostUrl) && !IsProcessing;
}
```

### View Code-Behind Mínimo

```csharp
// ✅ Code-behind apenas para inicialização
public partial class AnalysisView : UserControl
{
    public AnalysisView()
    {
        InitializeComponent();
    }
}

// ❌ EVITAR lógica de negócio no code-behind
// ❌ EVITAR manipulação direta de controles
```

### Comunicação entre ViewModels

```csharp
// ✅ Usar Messenger do CommunityToolkit
using CommunityToolkit.Mvvm.Messaging;

// Definir mensagem
public record AnalysisCompletedMessage(Guid AnalysisId);

// Enviar
WeakReferenceMessenger.Default.Send(new AnalysisCompletedMessage(analysis.Id));

// Receber
public class DashboardViewModel : ObservableObject, IRecipient<AnalysisCompletedMessage>
{
    public DashboardViewModel()
    {
        WeakReferenceMessenger.Default.Register<AnalysisCompletedMessage>(this);
    }

    public void Receive(AnalysisCompletedMessage message)
    {
        // Atualizar dashboard
        LoadRecentAnalysesAsync();
    }
}
```

---

## 8. Injeção de Dependências

### Configuração no App.xaml.cs

```csharp
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configurações
        services.Configure<ApiSettings>(Configuration.GetSection("ApiSettings"));
        services.Configure<CacheSettings>(Configuration.GetSection("CacheSettings"));

        // Infrastructure
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("Postgres")));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")));

        // Services
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IAnalysisService, AnalysisService>();
        services.AddScoped<ITwitterService, TwitterService>();
        services.AddScoped<IApiClient, ApiClient>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AnalysisViewModel>();
        services.AddTransient<HistoryViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<LoginView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<AnalysisView>();
        services.AddTransient<HistoryView>();

        // Helpers
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
```

### Lifetime Scopes

```csharp
// ✅ Singleton - Uma instância para toda aplicação
services.AddSingleton<ICacheService, RedisCacheService>();
services.AddSingleton<IDialogService, DialogService>();

// ✅ Scoped - Uma instância por scope (não muito usado em WPF)
services.AddScoped<ApplicationDbContext>();

// ✅ Transient - Nova instância a cada requisição
services.AddTransient<AnalysisViewModel>();
services.AddTransient<IAnalysisService, AnalysisService>();
```

---

## 9. Tratamento de Erros

### Try-Catch Pattern

```csharp
// ✅ Async com logging estruturado
public async Task<AnalysisResult> AnalyzePostAsync(string url)
{
    try
    {
        _logger.LogInformation("Iniciando análise para URL: {Url}", url);
        
        var postId = ExtractPostId(url);
        var tweet = await _twitterClient.GetTweetAsync(postId);
        var result = await ProcessTweetAsync(tweet);
        
        _logger.LogInformation("Análise concluída: {AnalysisId}", result.Id);
        return result;
    }
    catch (TwitterApiException ex)
    {
        _logger.LogError(ex, "Erro na API do Twitter: {Message}", ex.Message);
        throw new AnalysisException("Não foi possível acessar o post no Twitter", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro inesperado ao analisar post");
        throw;
    }
}
```

### Custom Exceptions

```csharp
// ✅ Exceções específicas do domínio
public class AnalysisException : Exception
{
    public AnalysisException(string message) : base(message) { }
    public AnalysisException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public class TwitterApiException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public TwitterApiException(int statusCode, string message, string? errorCode = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
```

### Global Exception Handler

```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // Capturar exceções não tratadas
    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    DispatcherUnhandledException += OnDispatcherUnhandledException;
    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
}

private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
{
    _logger.LogCritical(e.Exception, "Exceção não tratada na UI thread");
    
    MessageBox.Show(
        "Ocorreu um erro inesperado. A aplicação será encerrada.",
        "Erro Crítico",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
    
    e.Handled = true;
    Current.Shutdown();
}
```

---

## 10. Testes

### Estrutura de Testes

```csharp
// ✅ Usar xUnit + FluentAssertions + Moq
using Xunit;
using FluentAssertions;
using Moq;

public class AnalysisServiceTests
{
    private readonly Mock<ITwitterClient> _twitterClientMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<AnalysisService>> _loggerMock;
    private readonly AnalysisService _sut; // System Under Test

    public AnalysisServiceTests()
    {
        _twitterClientMock = new Mock<ITwitterClient>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<AnalysisService>>();
        
        _sut = new AnalysisService(
            _twitterClientMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AnalyzePostAsync_WithValidUrl_ShouldReturnAnalysis()
    {
        // Arrange
        var url = "https://x.com/user/status/123456789";
        var expectedTweet = new Tweet { /* ... */ };
        
        _twitterClientMock
            .Setup(x => x.GetTweetAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedTweet);

        // Act
        var result = await _sut.AnalyzePostAsync(url);

        // Assert
        result.Should().NotBeNull();
        result.PostId.Should().Be("123456789");
        result.Metrics.Should().NotBeEmpty();
        
        _twitterClientMock.Verify(x => x.GetTweetAsync("123456789"), Times.Once);
        _cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<AnalysisResult>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-url")]
    public async Task AnalyzePostAsync_WithInvalidUrl_ShouldThrowException(string invalidUrl)
    {
        // Act
        Func<Task> act = async () => await _sut.AnalyzePostAsync(invalidUrl);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

### Integration Tests

```csharp
// ✅ WebApplicationFactory para testes de API
public class AnalysisApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AnalysisApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_Analyze_ReturnsOkResult()
    {
        // Arrange
        var request = new AnalysisRequest("https://x.com/user/status/123456789", "quick");
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/analyze", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AnalysisResponse>();
        result.Should().NotBeNull();
        result!.PostId.Should().NotBeNullOrEmpty();
    }
}
```

---

## 11. Performance

### Async Best Practices

```csharp
// ✅ ConfigureAwait(false) em bibliotecas
public async Task<Tweet> GetTweetAsync(string tweetId)
{
    var response = await _httpClient.GetAsync($"/tweets/{tweetId}").ConfigureAwait(false);
    return await response.Content.ReadFromJsonAsync<Tweet>().ConfigureAwait(false);
}

// ✅ Parallel.ForEachAsync para operações I/O
public async Task ProcessMultipleAnalysesAsync(List<string> urls)
{
    await Parallel.ForEachAsync(urls, new ParallelOptions { MaxDegreeOfParallelism = 5 },
        async (url, ct) =>
        {
            await AnalyzePostAsync(url);
        });
}
```

### Caching

```csharp
// ✅ Cache distribuído (Redis)
public async Task<AnalysisResult?> GetCachedAnalysisAsync(string key)
{
    var cached = await _cache.GetStringAsync(key);
    return cached != null 
        ? JsonSerializer.Deserialize<AnalysisResult>(cached)
        : null;
}

public async Task SetCachedAnalysisAsync(string key, AnalysisResult analysis, TimeSpan ttl)
{
    var json = JsonSerializer.Serialize(analysis);
    await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = ttl
    });
}
```

### EF Core Optimization

```csharp
// ✅ AsNoTracking para leitura
public async Task<List<AnalysisDto>> GetUserAnalysesAsync(Guid userId)
{
    return await _context.Analyses
        .AsNoTracking()
        .Where(a => a.UserId == userId)
        .Select(a => new AnalysisDto
        {
            Id = a.Id,
            PostId = a.PostId,
            CreatedAt = a.CreatedAt
        })
        .ToListAsync();
}

// ✅ Paginação
public async Task<PagedResult<Analysis>> GetPagedAnalysesAsync(int page, int pageSize)
{
    var query = _context.Analyses.OrderByDescending(a => a.CreatedAt);
    
    var total = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Analysis>(items, total, page, pageSize);
}
```

---

## 12. Segurança

### Secrets Management

```json
// appsettings.Development.json (NÃO commitar!)
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;...",
    "Redis": "localhost:6379"
  },
  "Twitter": {
    "BearerToken": "AAAAAAAAAAAAAAAAAAAAAA..."
  }
}
```

```csharp
// ✅ User Secrets para desenvolvimento
// dotnet user-secrets set "Twitter:BearerToken" "your-token"

// ✅ Azure Key Vault para produção
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Input Validation

```csharp
// ✅ FluentValidation
public class AnalysisRequestValidator : AbstractValidator<AnalysisRequest>
{
    public AnalysisRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL é obrigatória")
            .Must(BeValidTwitterUrl).WithMessage("URL inválida do Twitter/X");

        RuleFor(x => x.Type)
            .Must(x => x is "quick" or "complete")
            .WithMessage("Tipo deve ser 'quick' ou 'complete'");
    }

    private bool BeValidTwitterUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Host.Contains("twitter.com") || uri.Host.Contains("x.com"));
    }
}
```

### SQL Injection Prevention

```csharp
// ✅ SEMPRE usar parâmetros
public async Task<User?> GetUserByEmailAsync(string email)
{
    // ✅ EF Core parametriza automaticamente
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email);
}

// ❌ NUNCA concatenar strings SQL
// var sql = $"SELECT * FROM Users WHERE Email = '{email}'"; // VULNERÁVEL!
```

---

## 13. Checklist de Código

### Antes de Commitar

#### C# Code
- [ ] `dotnet format` executado
- [ ] Nullable reference types habilitado e respeitado
- [ ] Sem `#pragma warning disable` desnecessários
- [ ] Async/await usado corretamente (sem `.Result` ou `.Wait()`)
- [ ] Logging estruturado com `ILogger`
- [ ] Exceções customizadas documentadas
- [ ] Unit tests passando (`dotnet test`)

#### XAML
- [ ] UTF-8 with BOM verificado
- [ ] StaticResource usado ao invés de DynamicResource (quando possível)
- [ ] DataContext definido via DI, não no XAML
- [ ] Estilos globais usados corretamente
- [ ] FluentIcons usado (sem emojis em code-behind)

#### Segurança
- [ ] Secrets não commitados (`.gitignore` atualizado)
- [ ] Input validation implementada
- [ ] Erros sensíveis não expostos ao usuário
- [ ] HTTPS obrigatório em produção

#### Performance
- [ ] `ConfigureAwait(false)` em bibliotecas
- [ ] `AsNoTracking()` em queries de leitura
- [ ] Cache implementado onde apropriado
- [ ] Paginação em listas grandes

#### Acessibilidade
- [ ] Tooltips em elementos não-óbvios
- [ ] Tab order lógico
- [ ] Contraste WCAG AA mínimo
- [ ] Tamanho mínimo de toque (28px)

---

## Recursos Adicionais

### Pacotes Recomendados

```xml
<!-- UI/MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="ModernWpfUI" Version="0.9.6" />
<PackageReference Include="FluentIcons.WPF" Version="1.1.251" />

<!-- Data/API -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />
<PackageReference Include="Refit" Version="7.0.0" />

<!-- Logging -->
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.6.6" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.70" />
```

### Ferramentas de Desenvolvimento

- **Visual Studio 2022** (v17.8+)
- **ReSharper** ou **Rider** (opcional, mas recomendado)
- **dotnet-format**: `dotnet tool install -g dotnet-format`
- **EF Core Tools**: `dotnet tool install -g dotnet-ef`

### Documentação Oficial

- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/)
- [WPF Documentation](https://learn.microsoft.com/dotnet/desktop/wpf/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

---

**Última atualização:** 13/10/2025  
**Versão:** 1.0  
**Mantenedor:** Equipe InfluenciAI