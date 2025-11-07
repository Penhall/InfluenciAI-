# InfluenciAI - Guia de Configuração

## 🚀 Pré-requisitos

- .NET 9.0 SDK
- PostgreSQL 15+
- Redis (opcional, para cache)
- RabbitMQ (opcional, para mensageria)
- Conta do Twitter Developer com credenciais OAuth 1.0a

## 🔐 Configuração de Credenciais

### 1. Copiar o template de configuração

```bash
cp src/Server/InfluenciAI.Api/appsettings.Development.json.template src/Server/InfluenciAI.Api/appsettings.Development.json
```

### 2. Configurar credenciais do Twitter

Edite `src/Server/InfluenciAI.Api/appsettings.Development.json`:

```json
"Twitter": {
    "ConsumerKey": "SUA_API_KEY_AQUI",
    "ConsumerSecret": "SEU_API_SECRET_AQUI",
    "AccessToken": "SEU_ACCESS_TOKEN_AQUI",
    "AccessTokenSecret": "SEU_ACCESS_TOKEN_SECRET_AQUI"
}
```

**Como obter as credenciais:**
1. Acesse https://developer.twitter.com/en/portal/dashboard
2. Selecione seu App
3. Vá em "Keys and tokens"
4. Copie:
   - **API Key** → ConsumerKey
   - **API Key Secret** → ConsumerSecret
   - **Access Token** → AccessToken
   - **Access Token Secret** → AccessTokenSecret

### 3. Configurar Banco de Dados

Atualize a connection string no `appsettings.Development.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=influenciai;Username=postgres;Password=SUA_SENHA"
}
```

### 4. Configurar JWT Secret

```json
"Jwt": {
    "Key": "SUA_CHAVE_SECRETA_MINIMO_32_CARACTERES"
}
```

### 5. Configurar senha do Admin

```json
"Seed": {
    "Admin": {
        "Password": "SUA_SENHA_ADMIN",
        "Email": "admin@local"
    }
}
```

## 🗄️ Configurar Banco de Dados

### 1. Criar o banco de dados

```bash
createdb -U postgres influenciai
```

Ou via psql:
```sql
CREATE DATABASE influenciai;
```

### 2. Aplicar migrations

```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

dotnet ef database update \
    --project src/Infra/InfluenciAI.Infrastructure \
    --startup-project src/Server/InfluenciAI.Api
```

## ▶️ Executar o Projeto

### Backend (API)

```bash
cd src/Server/InfluenciAI.Api
dotnet run
```

A API estará disponível em: http://localhost:5000
Swagger UI: http://localhost:5000/swagger

### Desktop (WPF)

```bash
cd src/Client/InfluenciAI.Desktop
dotnet run
```

## 🧪 Testar a API

### 1. Health Check

```bash
curl http://localhost:5000/health/live
```

### 2. Criar Tenant

```bash
curl -X POST http://localhost:5000/api/tenants \
  -H "Content-Type: application/json" \
  -d '{"name": "Meu Tenant"}'
```

### 3. Login

```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@local", "password": "SUA_SENHA_ADMIN"}'
```

### 4. Conectar perfil do Twitter

```bash
curl -X POST http://localhost:5000/api/social-profiles/connect \
  -H "Authorization: Bearer SEU_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "network": 1,
    "accessToken": "seu_access_token",
    "refreshToken": null,
    "tokenExpiresAt": "2025-12-31T23:59:59Z"
  }'
```

### 5. Criar e publicar tweet

```bash
# Criar conteúdo
curl -X POST http://localhost:5000/api/content \
  -H "Authorization: Bearer SEU_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Meu primeiro tweet",
    "body": "Olá mundo! Tweet via InfluenciAI",
    "type": 1
  }'

# Publicar
curl -X POST http://localhost:5000/api/content/{contentId}/publish \
  -H "Authorization: Bearer SEU_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "socialProfileId": "seu_profile_id"
  }'
```

## 📊 Endpoints Disponíveis

- `GET /health/live` - Health check
- `GET /health/ready` - Readiness check
- `POST /auth/login` - Login
- `POST /auth/register` - Registro
- `GET /api/tenants` - Listar tenants
- `GET /api/social-profiles` - Listar perfis conectados
- `POST /api/social-profiles/connect` - Conectar rede social
- `GET /api/content` - Listar conteúdos
- `POST /api/content` - Criar conteúdo
- `POST /api/content/{id}/publish` - Publicar conteúdo
- `GET /api/content/{id}/metrics` - Métricas do conteúdo

## 🔧 Troubleshooting

### Erro: "Twitter:ConsumerKey not configured"

Verifique se o arquivo `appsettings.Development.json` está configurado corretamente e **não está no .gitignore** localmente.

### Erro: "Connection refused" ao conectar no PostgreSQL

1. Verifique se o PostgreSQL está rodando: `sudo systemctl status postgresql`
2. Verifique a connection string
3. Teste a conexão: `psql -U postgres -h localhost`

### Erro: "Failed to publish tweet"

1. Verifique as credenciais do Twitter no appsettings
2. Verifique se o App tem permissões de "Read and Write"
3. Verifique os logs para mais detalhes

## 📚 Documentação

Mais detalhes em:
- `docs/InfluenciAI-Documentation/`
- `docs/InfluenciAI-Documentation/11-ProjectManagement/11.4-MVP-SingleNetworkPublisher.md`

## 🔒 Segurança

⚠️ **NUNCA commite arquivos com credenciais reais!**

Os seguintes arquivos estão no `.gitignore`:
- `appsettings.Development.json`
- `appsettings.Production.json`
- `docs/**/env-*.md`

Use o template `.template` como referência.
