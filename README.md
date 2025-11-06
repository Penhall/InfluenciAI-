# Documentação Técnica Consolidada — InfluenciAI

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

Status Atual (05/11/2025)

**Fase 1 - Foundation: ~80% Completa** ✅

Implementado:
- API Minimal com Identity + JWT, CORS dinâmico por tenant, HealthChecks e Telemetria.
- Fluxo de autenticação com refresh tokens (`/auth/login`, `/auth/refresh`, `/auth/logout`, `/auth/logout/all`).
- Módulo Tenants (CRUD via CQRS/MediatR) + testes unitários.
- Desktop (WPF) com telas de Login, Tenants e Usuários, e auto-refresh de token.
- Docker Compose com PostgreSQL, Redis, RabbitMQ.
- Execução local documentada em `docs/InfluenciAI-Documentation/05-Development/05.2-LocalRun.md`.

**Pendências da Fase 1:**
- Pipeline CI/CD com quality gates
- Externalizar segredos (JWT Key, connection strings)
- Estabilizar testes de integração
- Job de limpeza de refresh tokens expirados

**Próxima Fase: MVP "Single Network Publisher"** 🚀

Objetivo: Implementar o primeiro fluxo end-to-end de valor de negócio:
- Integração com Twitter/X (OAuth + publicação)
- Coleta básica de métricas (views, likes, retweets)
- Visualização de métricas no Desktop

Estimativa: 4-6 semanas (Sprints 1-6)

Ver detalhes em `docs/InfluenciAI-Documentation/11-ProjectManagement/11.2-ProjectPlan.md`
