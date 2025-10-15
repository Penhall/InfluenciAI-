# DocumentaÃ§Ã£o TÃ©cnica Consolidada â€” InfluenciAI (Atualizado .NET 10 RC)

Este diretÃ³rio organiza, em formato tÃ©cnico e navegÃ¡vel, os principais aspectos da soluÃ§Ã£o InfluenciAI. O conteÃºdo foi consolidado a partir dos documentos existentes em `docs/MotivaÃ§Ã£o` e segue as regras de estilo e desenvolvimento definidas em `.rules/`.

Estrutura:

- 03-Architecture
  - 03.1-SolutionArchitecture.md
  - 03.2-TechnicalArchitecture.md
  - 03.5-IntegrationArchitecture.md
  - 03.6-APIEndpoints.md
  - 03.7-OperationalFlows.md
- 04-Design
  - 04.1-DomainModel.md
- 05-Development
  - 05.1-DevelopmentStandards.md
- 06-Infrastructure
  - 06.1-EnvironmentSetup.md
- 09-Deployment
  - 09.1-DeploymentPlan.md
- 11-ProjectManagement
  - 11.2-ProjectPlan.md
  - 11.3-BacklogDefinition.md

MudanÃ§as recentes relevantes:

- Alvo .NET 10 (RC) nos serviÃ§os e testes; WPF em `net10.0-windows`
- ASP.NET Core Identity + JWT com roles/polÃ­ticas (ex.: `TenantAdmin`)
- CORS dinÃ¢mico por tenant (`X-Tenant-Id` + `Cors:Tenants:{id}:AllowedOrigins`)
- HealthChecks UI exposta em `/health/ui`

ReferÃªncias origem (principais):

- `docs/MotivaÃ§Ã£o/technical-architecture.md`
- `docs/MotivaÃ§Ã£o/influenciai-solution-structure.md`
- `docs/MotivaÃ§Ã£o/influenciai-modular-architecture.md`
- `docs/MotivaÃ§Ã£o/influenciai-business-rules.md`
- `docs/MotivaÃ§Ã£o/implementation-phases.md`
- `docs/MotivaÃ§Ã£o/modular-implementation-roadmap.md`
- `docs/MotivaÃ§Ã£o/roadmap-release-plan.md`
- `docs/MotivaÃ§Ã£o/complete-project-backlog.md`

