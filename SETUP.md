# InfluenciAI - Setup de Desenvolvimento

## Pré-requisitos

- ✅ .NET 9.0 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- ✅ Docker Desktop ([Download](https://www.docker.com/products/docker-desktop))
- ✅ Visual Studio 2022 (ou VS Code)
- ✅ Git

## Setup Rápido (5 minutos)

### 1. Clonar o Repositório

```bash
git clone https://github.com/seu-usuario/InfluenciAI.git
cd InfluenciAI
```

### 2. Configurar Segredos

Execute o script PowerShell:

```powershell
.\scripts\setup-secrets.ps1
```

O script irá solicitar:
- Senha do PostgreSQL (padrão: `postgres`)
- Senha do usuário admin (padrão: `Admin!234`)

### 3. Subir Dependências (Docker)

```powershell
docker compose up -d
```

Isso iniciará:
- PostgreSQL (porta 5432)
- Redis (porta 6379)
- RabbitMQ (portas 5672, 15672)

### 4. Aplicar Migrations

```powershell
dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api
```

### 5. Rodar a Aplicação

**Opção A: Visual Studio**
1. Abrir `InfluenciAI.sln`
2. Configurar Multiple Startup Projects (API + Desktop)
3. Pressionar F5

**Opção B: CLI**
```powershell
# Terminal 1: API
dotnet run --project src/Server/InfluenciAI.Api

# Terminal 2: Desktop
dotnet run --project src/Client/InfluenciAI.Desktop
```

## Verificar Setup

### API
- Swagger: http://localhost:5228/swagger
- HealthCheck: http://localhost:5228/health/live

### Desktop
- Login: `admin@local` / `Admin!234`

### Docker
```powershell
docker ps
```

Deve mostrar 3 containers rodando:
- `influenciai_postgres`
- `influenciai_redis`
- `influenciai_rabbit`

## Estrutura do Projeto

```
InfluenciAI/
├── src/
│   ├── Core/
│   │   ├── InfluenciAI.Domain/         # Entidades e lógica de negócio
│   │   └── InfluenciAI.Application/    # Use cases (CQRS)
│   ├── Infra/
│   │   └── InfluenciAI.Infrastructure/ # EF Core, Identity, Integrations
│   ├── Server/
│   │   └── InfluenciAI.Api/            # API REST
│   └── Client/
│       └── InfluenciAI.Desktop/        # WPF Client
├── tests/
│   └── InfluenciAI.Tests/              # Testes unitários e integração
├── docs/                               # Documentação técnica
├── scripts/                            # Scripts de automação
└── docker-compose.yml                  # Dependências (PostgreSQL, Redis, RabbitMQ)
```

## Comandos Úteis

### Docker

```powershell
# Parar containers
docker compose down

# Parar e remover volumes (reset completo)
docker compose down -v

# Ver logs
docker logs influenciai_postgres
docker logs influenciai_redis
docker logs influenciai_rabbit
```

### Entity Framework

```powershell
# Criar migration
dotnet ef migrations add NomeDaMigration --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api

# Aplicar migrations
dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api

# Reverter migration
dotnet ef database update PreviousMigrationName --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api

# Listar migrations
dotnet ef migrations list --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api
```

### Testes

```powershell
# Rodar todos os testes
dotnet test

# Rodar testes com coverage
dotnet test /p:CollectCoverage=true

# Rodar testes específicos
dotnet test --filter "FullyQualifiedName~Tenants"
```

### User Secrets

```powershell
# Ver segredos configurados
dotnet user-secrets list --project src/Server/InfluenciAI.Api

# Adicionar/atualizar segredo
dotnet user-secrets set "Chave" "Valor" --project src/Server/InfluenciAI.Api

# Remover segredo
dotnet user-secrets remove "Chave" --project src/Server/InfluenciAI.Api
```

## Troubleshooting

### Erro: "Cannot connect to PostgreSQL"

**Solução:**
```powershell
# Verificar se container está rodando
docker ps | findstr postgres

# Se não estiver, subir
docker compose up -d

# Ver logs
docker logs influenciai_postgres
```

### Erro: "A configuration item with the key 'Jwt:Key' was not found"

**Solução:** Configurar User Secrets
```powershell
.\scripts\setup-secrets.ps1
```

### Erro: "Pending migrations"

**Solução:**
```powershell
dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api
```

### Desktop não conecta na API

**Verificar:**
1. API está rodando? (http://localhost:5228/swagger)
2. Arquivo `src/Client/InfluenciAI.Desktop/appsettings.json` tem a URL correta?

```json
{
  "ApiBaseUrl": "http://localhost:5228"
}
```

## Próximos Passos

Após setup concluído:

1. **Explorar o Swagger:** http://localhost:5228/swagger
2. **Fazer login no Desktop:** `admin@local` / `Admin!234`
3. **Ler a documentação:** `docs/InfluenciAI-Documentation/`
4. **Ver o backlog:** `docs/InfluenciAI-Documentation/11-ProjectManagement/11.3-BacklogDefinition.md`

## Suporte

- **Documentação completa:** `docs/InfluenciAI-Documentation/`
- **Issues:** https://github.com/seu-usuario/InfluenciAI/issues
- **Wiki:** https://github.com/seu-usuario/InfluenciAI/wiki
