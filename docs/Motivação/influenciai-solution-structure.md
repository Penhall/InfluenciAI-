# Estrutura da Solução InfluenciAI .NET

## 📁 InfluenciAI.sln
```
InfluenciAI/
│
├── src/
│   ├── 1-Core/
│   │   ├── InfluenciAI.Domain/                    # Entidades e Agregados
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Analysis.cs
│   │   │   │   ├── Metric.cs
│   │   │   │   └── Recommendation.cs
│   │   │   ├── Enums/
│   │   │   │   ├── UserPlan.cs
│   │   │   │   └── AnalysisType.cs
│   │   │   ├── ValueObjects/
│   │   │   │   ├── Email.cs
│   │   │   │   └── PostUrl.cs
│   │   │   └── Specifications/
│   │   │
│   │   ├── InfluenciAI.Application/               # Casos de Uso e DTOs
│   │   │   ├── Commands/
│   │   │   │   ├── Analysis/
│   │   │   │   │   ├── CreateAnalysisCommand.cs
│   │   │   │   │   └── CreateAnalysisHandler.cs
│   │   │   │   └── Auth/
│   │   │   │       ├── LoginCommand.cs
│   │   │   │       └── RegisterCommand.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetAnalysisQuery.cs
│   │   │   │   └── GetUserAnalysesQuery.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── AnalysisDto.cs
│   │   │   │   ├── MetricDto.cs
│   │   │   │   └── UserDto.cs
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAnalysisService.cs
│   │   │   │   ├── IAuthService.cs
│   │   │   │   └── ICacheService.cs
│   │   │   └── Validators/
│   │   │       └── AnalysisValidator.cs
│   │   │
│   │   └── InfluenciAI.Contracts/                 # Contratos Compartilhados
│   │       ├── Requests/
│   │       │   ├── AnalyzePostRequest.cs
│   │       │   └── LoginRequest.cs
│   │       ├── Responses/
│   │       │   ├── AnalysisResponse.cs
│   │       │   └── AuthResponse.cs
│   │       └── Events/
│   │           └── AnalysisCompletedEvent.cs
│   │
│   ├── 2-Infrastructure/
│   │   ├── InfluenciAI.Infrastructure.Data/       # Entity Framework Core
│   │   │   ├── Context/
│   │   │   │   └── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   └── AnalysisConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── UserRepository.cs
│   │   │   │   └── AnalysisRepository.cs
│   │   │   └── Migrations/
│   │   │
│   │   ├── InfluenciAI.Infrastructure.Cache/      # Redis
│   │   │   ├── Services/
│   │   │   │   └── RedisCacheService.cs
│   │   │   └── Configuration/
│   │   │       └── RedisOptions.cs
│   │   │
│   │   ├── InfluenciAI.Infrastructure.Identity/   # ASP.NET Identity
│   │   │   ├── Services/
│   │   │   │   ├── IdentityService.cs
│   │   │   │   └── JwtService.cs
│   │   │   └── Configuration/
│   │   │       └── IdentityConfiguration.cs
│   │   │
│   │   └── InfluenciAI.Infrastructure.External/   # Integrações Externas
│   │       ├── Twitter/
│   │       │   ├── TwitterService.cs
│   │       │   └── TwitterConfiguration.cs
│   │       ├── AI/
│   │       │   ├── OpenAIService.cs
│   │       │   └── MockAIService.cs
│   │       └── OAuth/
│   │           ├── GoogleOAuthProvider.cs
│   │           └── GitHubOAuthProvider.cs
│   │
│   ├── 3-Services/
│   │   ├── InfluenciAI.Api.Gateway/               # API Gateway Principal
│   │   │   ├── Controllers/
│   │   │   │   ├── AuthController.cs
│   │   │   │   ├── AnalysisController.cs
│   │   │   │   └── AdminController.cs
│   │   │   ├── GraphQL/
│   │   │   │   ├── Schema/
│   │   │   │   ├── Queries/
│   │   │   │   └── Mutations/
│   │   │   ├── Middleware/
│   │   │   │   ├── ExceptionMiddleware.cs
│   │   │   │   └── RateLimitMiddleware.cs
│   │   │   └── Program.cs
│   │   │
│   │   ├── InfluenciAI.Service.Analysis/          # Microserviço de Análise
│   │   │   ├── Services/
│   │   │   │   ├── AnalysisOrchestrator.cs
│   │   │   │   ├── MetricsCalculator.cs
│   │   │   │   └── RecommendationEngine.cs
│   │   │   └── Workers/
│   │   │       └── AnalysisBackgroundService.cs
│   │   │
│   │   └── InfluenciAI.Service.Reporting/         # Microserviço de Relatórios
│   │       ├── Services/
│   │       │   ├── ReportGenerator.cs
│   │       │   └── ExportService.cs
│   │       └── Templates/
│   │
│   ├── 4-Presentation/
│   │   ├── InfluenciAI.Desktop.WPF/               # Cliente Desktop Principal
│   │   │   ├── Views/
│   │   │   │   ├── LoginView.xaml
│   │   │   │   ├── DashboardView.xaml
│   │   │   │   ├── AnalysisView.xaml
│   │   │   │   └── HistoryView.xaml
│   │   │   ├── ViewModels/
│   │   │   │   ├── LoginViewModel.cs
│   │   │   │   ├── DashboardViewModel.cs
│   │   │   │   └── AnalysisViewModel.cs
│   │   │   ├── Services/
│   │   │   │   ├── ApiClient.cs
│   │   │   │   └── NavigationService.cs
│   │   │   ├── Controls/
│   │   │   │   ├── MetricCard.xaml
│   │   │   │   └── ChartControl.xaml
│   │   │   ├── Converters/
│   │   │   ├── Resources/
│   │   │   │   └── Themes/
│   │   │   └── App.xaml.cs
│   │   │
│   │   ├── InfluenciAI.Desktop.Core/              # Lógica Compartilhada Desktop
│   │   │   ├── Models/
│   │   │   ├── Helpers/
│   │   │   └── Extensions/
│   │   │
│   │   └── InfluenciAI.Web/                       # Interface Web (Futuro)
│   │       └── (Blazor ou MVC)
│   │
│   └── 5-CrossCutting/
│       ├── InfluenciAI.Common/                    # Utilitários Comuns
│       │   ├── Extensions/
│       │   ├── Helpers/
│       │   └── Constants/
│       │
│       └── InfluenciAI.Logging/                   # Logging Centralizado
│           ├── Serilog/
│           └── Telemetry/
│
├── tests/
│   ├── InfluenciAI.Domain.Tests/
│   ├── InfluenciAI.Application.Tests/
│   ├── InfluenciAI.Infrastructure.Tests/
│   ├── InfluenciAI.Api.Tests/
│   └── InfluenciAI.Desktop.Tests/
│
├── tools/
│   ├── DataMigration/                             # Ferramentas de Migração
│   │   ├── SqliteToPostgres/
│   │   └── DataSeeder/
│   └── Scripts/
│       ├── build.ps1
│       └── deploy.ps1
│
└── docs/
    ├── Architecture/
    ├── API/
    └── UserGuides/
```

## 📋 Responsabilidades por Projeto

### Core Layer
- **Domain**: Regras de negócio puras, sem dependências externas
- **Application**: Orquestração de casos de uso, validações
- **Contracts**: DTOs e interfaces compartilhadas

### Infrastructure Layer
- **Data**: Persistência com EF Core e PostgreSQL
- **Cache**: Implementação Redis
- **Identity**: Autenticação e autorização
- **External**: Integrações com APIs externas

### Services Layer
- **Gateway**: Ponto único de entrada, roteamento
- **Analysis Service**: Processamento de análises
- **Reporting Service**: Geração de relatórios

### Presentation Layer
- **Desktop.WPF**: Cliente principal Windows
- **Desktop.Core**: Lógica reutilizável entre clientes
- **Web**: Interface web futura

### Cross-Cutting
- **Common**: Código compartilhado entre todos os projetos
- **Logging**: Telemetria e observabilidade

## 🔧 Configurações por Projeto

### Pacotes NuGet Principais

#### Domain
```xml
<PackageReference Include="FluentValidation" Version="11.9.0" />
```

#### Application
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
```

#### Infrastructure.Data
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

#### Infrastructure.Cache
```xml
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />
```

#### Infrastructure.Identity
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
```

#### Infrastructure.External
```xml
<PackageReference Include="TweetinviAPI" Version="5.1.0" />
<PackageReference Include="RestSharp" Version="110.2.0" />
```

#### API Gateway
```xml
<PackageReference Include="HotChocolate.AspNetCore" Version="13.8.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
```

#### Desktop.WPF
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="ModernWpfUI" Version="0.9.6" />
<PackageReference Include="LiveCharts2" Version="2.0.0" />
<PackageReference Include="Refit" Version="7.0.0" />
```
