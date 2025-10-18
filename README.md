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

Status Atual (MVP Foundation)
- API Minimal com Identity + JWT, CORS dinâmico por tenant, HealthChecks e Telemetria.
- Fluxo de autenticação com refresh tokens (`/auth/login`, `/auth/refresh`, `/auth/logout`, `/auth/logout/all`).
- Módulo Tenants (CRUD via CQRS/MediatR) + testes.
- Desktop (WPF) com telas de Login, Tenants e Usuários, e auto-refresh de token.
- Execução local documentada em `docs/InfluenciAI-Documentation/05-Development/05.2-LocalRun.md`.

Próximos Passos
- Estabilizar testes de integração (WebApplicationFactory + EF InMemory compartilhado) e ampliar cobertura.
- Configurar pipeline CI (build/test/quality) e preparar variáveis/segredos por ambiente.
- Endurecer segurança: segredos fora de arquivos, revisar políticas de refresh e adicionar limpeza de tokens.
- Melhorias de UX no Desktop: mensagens de erro, sessão expirada e navegação.
