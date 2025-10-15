# Changelog da Documentação — InfluenciAI

Data: 2025-10-15

Resumo das mudanças relevantes na documentação técnica após a atualização para .NET 10 (RC), segurança e operabilidade:

- Adicionado: 03-Architecture/03.6-APIEndpoints.md (mapa dos endpoints: saúde, auth, tenants, users)
- Adicionado: HealthChecks UI referenciado em `/health/ui` e detalhes de readiness
- Adicionado: CORS dinâmico por tenant (provider customizado) e orientações de configuração
- Atualizado: 03.2-TechnicalArchitecture.md (alvo .NET 10 RC, Identity + JWT, CORS dinâmico, HealthChecks UI)
- Atualizado: 03.1-SolutionArchitecture.md (Minimal API com políticas/roles e Identity)
- Atualizado: 06.1-EnvironmentSetup.md (SDK .NET 10 RC, design-time EF, HealthChecks UI)
- Atualizado: 05.1-DevelopmentStandards.md (Minimal API, políticas/roles, testes de CORS)
- Atualizado: 09.1-DeploymentPlan.md (marcação de HealthChecks UI e alinhamento para GA)
- Atualizado: 11.2-ProjectPlan.md (Identity + JWT e CORS dinâmico na Fase 1; risco RC→GA)
- Adicionado: README do diretório de documentação com índice consolidado

Observações:

- A pilha de pacotes usa versões RC para .NET 10/EF e Npgsql EF; planejar atualização para GA assim que disponível.
- Diagramas operacionais (Mermaid) adicionados para fluxos de Autenticação, CORS e Health (ver 03.7-OperationalFlows.md).

