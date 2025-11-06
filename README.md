# InfluenciAI

[![CI/CD Pipeline](https://github.com/seu-usuario/InfluenciAI/actions/workflows/ci.yml/badge.svg)](https://github.com/seu-usuario/InfluenciAI/actions/workflows/ci.yml)
[![CodeQL](https://github.com/seu-usuario/InfluenciAI/actions/workflows/codeql.yml/badge.svg)](https://github.com/seu-usuario/InfluenciAI/actions/workflows/codeql.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Plataforma integrada de gestão de mídia social** para publicação, análise e otimização de conteúdo em múltiplas redes sociais.

---

## 📚 Documentação

Este diretório organiza, em formato técnico e navegável, os principais aspectos da solução InfluenciAI. O conteúdo foi consolidado a partir dos documentos existentes em `docs/Motivação` e segue as regras de estilo e desenvolvimento definidas em `.rules/`.

Estrutura sugerida (primeiros entregáveis já incluídos):

- 03-Architecture
  - 03.1-SolutionArchitecture.md
  - 03.2-TechnicalArchitecture.md
  - 03.5-IntegrationArchitecture.md
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

Referências origem (principais):

- `docs/Motivação/technical-architecture.md`
- `docs/Motivação/influenciai-solution-structure.md`
- `docs/Motivação/influenciai-modular-architecture.md`
- `docs/Motivação/influenciai-business-rules.md`
- `docs/Motivação/implementation-phases.md`
- `docs/Motivação/modular-implementation-roadmap.md`
- `docs/Motivação/roadmap-release-plan.md`
- `docs/Motivação/complete-project-backlog.md`

Regras e estilo:

- Linguagem: documentos em Português (BR). Código/fonte em Inglês. Ver `.rules/rules.md`.
- Padrões visuais WPF/XAML: ver `.rules/style_guide.md`.

## 🚀 Quick Start

```bash
# 1. Configurar segredos
.\scripts\setup-secrets.ps1

# 2. Subir dependências
docker compose up -d

# 3. Aplicar migrations
dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api

# 4. Rodar
dotnet run --project src/Server/InfluenciAI.Api  # API
dotnet run --project src/Client/InfluenciAI.Desktop  # Desktop
```

**Ver guia completo:** [SETUP.md](SETUP.md)

---

## 📊 Status Atual (06/11/2025)

### **Fase 1 - Foundation: ~90% Completa** ✅

**Implementado:**
- ✅ API Minimal com Identity + JWT, CORS dinâmico por tenant, HealthChecks e Telemetria
- ✅ Fluxo de autenticação com refresh tokens (`/auth/login`, `/auth/refresh`, `/auth/logout`, `/auth/logout/all`)
- ✅ Módulo Tenants (CRUD via CQRS/MediatR) + testes unitários
- ✅ Desktop (WPF) com telas de Login, Tenants e Usuários, e auto-refresh de token
- ✅ Docker Compose com PostgreSQL, Redis, RabbitMQ
- ✅ Entidades de domínio para redes sociais (SocialProfile, Content, Publication, MetricSnapshot)
- ✅ **Downgrade para .NET 9.0 LTS** (versão estável)
- ✅ **Segredos externalizados** (User Secrets + script de setup)
- ✅ **Pipeline CI/CD** (GitHub Actions com build, test, quality gates)

**Pendências da Fase 1:**
- ⏳ Estabilizar testes de integração (60% → 80%)
- ⏳ Job de limpeza de refresh tokens expirados
- ⏳ Melhorias de UX no Desktop (validações, feedback sessão expirada)

### **Próxima Fase: MVP "Single Network Publisher"** 🚀

**Objetivo:** Implementar o primeiro fluxo end-to-end de valor de negócio:
- Integração com Twitter/X (OAuth + publicação)
- Coleta básica de métricas (views, likes, retweets)
- Visualização de métricas no Desktop

**Estimativa:** 4-6 semanas (Sprints 1-6)

**Ver detalhes:** [`11.2-ProjectPlan.md`](docs/InfluenciAI-Documentation/11-ProjectManagement/11.2-ProjectPlan.md) | [`11.4-MVP-SingleNetworkPublisher.md`](docs/InfluenciAI-Documentation/11-ProjectManagement/11.4-MVP-SingleNetworkPublisher.md)

---

## 🏗️ Arquitetura

**Clean Architecture** com CQRS + Event-Driven patterns

```
src/
├── Core/
│   ├── Domain/          # Entidades, Value Objects
│   └── Application/     # Use Cases (CQRS/MediatR)
├── Infra/
│   └── Infrastructure/  # EF Core, Identity, Integrations
├── Server/
│   └── Api/             # REST API (Minimal API)
└── Client/
    └── Desktop/         # WPF Desktop App
```

**Stack Técnica:**
- .NET 9.0 LTS (C#)
- ASP.NET Core + Minimal API
- Entity Framework Core + PostgreSQL
- Redis (cache)
- RabbitMQ (mensageria)
- WPF + MVVM

---

## 📖 Documentação Completa

Ver [`docs/InfluenciAI-Documentation/`](docs/InfluenciAI-Documentation/)
