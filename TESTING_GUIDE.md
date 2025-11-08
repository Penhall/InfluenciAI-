# Guia de Testes - Sistema de Métricas do Twitter

## 🚀 Início Rápido

### 1. Iniciar Ambiente

```bash
# Iniciar Docker Compose + Migrations
./scripts/start-dev.sh

# Iniciar API (em outro terminal)
cd src/Server/InfluenciAI.Api
dotnet run
```

A API estará disponível em: `http://localhost:5228`

### 2. Autenticação

```bash
# Login
curl -X POST http://localhost:5228/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin123!"}'

# Resposta:
{
  "access_token": "eyJhbG...",
  "refresh_token": "NOT..."
}
```

**Credenciais Padrão:**
- Email: `admin@local`
- Senha: `Admin123!`
- Tenant: `Default`

## 🔍 Testando o Sistema de Métricas

### 3. Criar Conteúdo

```bash
export TOKEN="SEU_ACCESS_TOKEN_AQUI"

curl -X POST http://localhost:5228/api/content \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Teste de Métricas",
    "body": "Este é um tweet de teste para coleta de métricas! #InfluenciAI",
    "status": "draft"
  }'

# Resposta contém o contentId
```

### 4. Criar Perfil Social (Twitter)

```bash
curl -X POST http://localhost:5228/api/social-profiles \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "network": "Twitter",
    "profileName": "Meu Twitter",
    "accessToken": "dummy_token_will_use_config"
  }'

# Resposta contém o profileId
```

### 5. Publicar no Twitter

```bash
export CONTENT_ID="seu-content-id-aqui"
export PROFILE_ID="seu-profile-id-aqui"

curl -X POST http://localhost:5228/api/content/$CONTENT_ID/publish \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "socialProfileId": "'$PROFILE_ID'"
  }'

# Resposta:
{
  "publicationId": "...",
  "tweetId": "...",
  "tweetUrl": "https://twitter.com/user/status/..."
}
```

### 6. Consultar Métricas

**Imediatamente após publicação:**
```bash
curl -X GET http://localhost:5228/api/content/$CONTENT_ID/metrics \
  -H "Authorization: Bearer $TOKEN" | jq
```

**Resposta esperada:**
```json
{
  "contentId": "...",
  "title": "Teste de Métricas",
  "body": "Este é um tweet de teste...",
  "publishedAt": "2025-11-07T...",
  "latestMetrics": {
    "id": "...",
    "publicationId": "...",
    "collectedAt": "2025-11-07T...",
    "views": 0,
    "likes": 0,
    "shares": 0,
    "comments": 0,
    "engagementRate": 0.0,
    "impressions": 0,
    "clicks": 0
  },
  "timeseriesMetrics": [
    // Array com histórico de coletas
  ]
}
```

## ⏱️ Coleta Automática de Métricas

O `DataCollectorService` coleta métricas automaticamente:

### Estratégia de Coleta

| Tempo desde publicação | Frequência de Coleta |
|------------------------|---------------------|
| 0 - 2 horas           | A cada 5 minutos    |
| 2 - 6 horas           | A cada 30 minutos   |
| 6 - 24 horas          | A cada 1 hora       |
| > 24 horas            | Não coleta mais     |

### Verificar Logs do Coletor

```bash
# Verificar se o coletor está rodando
# Procure por estas mensagens nos logs da API:

# [INF] DataCollectorService started
# [INF] Found X recent publications to collect metrics for
# [INF] Collecting metrics for publication {...}, tweet {...}
# [INF] Saved metrics snapshot for publication {...}: Likes=X, Retweets=Y
```

### Acompanhar Coleta em Tempo Real

```bash
# Aguarde 5 minutos após publicar
# Consulte as métricas novamente

curl -X GET http://localhost:5228/api/content/$CONTENT_ID/metrics \
  -H "Authorization: Bearer $TOKEN" | jq '.timeseriesMetrics | length'

# O número de snapshots deve aumentar a cada coleta
```

## 🐦 Configuração do Twitter

As credenciais já estão em `appsettings.Development.json`:

```json
{
  "Twitter": {
    "ConsumerKey": "...",
    "ConsumerSecret": "...",
    "AccessToken": "...",
    "AccessTokenSecret": "...",
    "BearerToken": "..."
  }
}
```

**Importante:** Atualmente usa OAuth 1.0a com credenciais compartilhadas. Para produção, implementar OAuth flow por usuário.

## ⚠️ Limitações Conhecidas

### Twitter API v1.1

- ❌ **Views**: Não disponível na v1.1
- ❌ **Replies count**: Não diretamente disponível
- ✅ **Likes**: Funciona
- ✅ **Retweets**: Funciona
- ✅ **Quotes**: Funciona (limitado)

Para obter métricas completas, considerar upgrade para **Twitter API v2**.

## 🗄️ Verificando o Banco de Dados

```bash
# Conectar ao PostgreSQL
docker exec -it influenciai_postgres psql -U postgres -d influenciai

# Ver publicações recentes
SELECT id, content_id, status, published_at, external_id
FROM publications
ORDER BY published_at DESC
LIMIT 5;

# Ver snapshots de métricas
SELECT
    publication_id,
    collected_at,
    views,
    likes,
    shares,
    comments,
    engagement_rate
FROM metric_snapshots
ORDER BY collected_at DESC
LIMIT 10;

# Ver evolução de métricas de uma publicação específica
SELECT
    collected_at,
    likes,
    shares,
    comments
FROM metric_snapshots
WHERE publication_id = 'SEU_PUBLICATION_ID'
ORDER BY collected_at ASC;
```

## 🧪 Testes Automatizados

### Testar Endpoints Principais

```bash
# 1. Health check (esperado 404, mas confirma que API responde)
curl -s http://localhost:5228/health

# 2. Login
curl -X POST http://localhost:5228/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin123!"}'

# 3. Criar conteúdo
curl -X POST http://localhost:5228/api/content \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Test","body":"Test body","status":"draft"}'

# 4. Buscar métricas (deve retornar 404 ou dados vazios se não publicado)
curl -X GET http://localhost:5228/api/content/$CONTENT_ID/metrics \
  -H "Authorization: Bearer $TOKEN"
```

## 📊 Monitoramento

### Logs Importantes

```bash
# Ver logs da API em tempo real
# (se rodando em background, parar e rodar em foreground)

cd src/Server/InfluenciAI.Api
dotnet run

# Procure por:
# - [INF] Seed completed
# - [INF] DataCollectorService started
# - [INF] Collecting metrics for publication
# - [ERR] (qualquer erro)
```

### Containers Docker

```bash
# Ver status
docker-compose ps

# Ver logs do PostgreSQL
docker logs influenciai_postgres

# Ver logs do Redis
docker logs influenciai_redis

# Ver logs do RabbitMQ
docker logs influenciai_rabbit
```

## 🔧 Troubleshooting

### Problema: API não conecta ao PostgreSQL

```bash
# Verificar se PostgreSQL está rodando
docker ps | grep postgres

# Verificar connection string
cat src/Server/InfluenciAI.Api/appsettings.Development.json | grep DefaultConnection
```

### Problema: DataCollectorService não coleta métricas

1. Verificar se há publicações nas últimas 24h
2. Verificar logs para erros de API do Twitter
3. Verificar credenciais do Twitter em `appsettings.Development.json`

### Problema: Twitter API retorna erros

```bash
# Verificar se as credenciais estão corretas
# Verificar rate limits do Twitter
# Twitter API v1.1 tem limites estritos

# Logs mostrarão:
# [ERR] Failed to get tweet metrics: ...
```

## 🎯 Próximos Passos

1. **Dashboard de Visualização**
   - Criar gráficos de evolução temporal
   - Comparar performance entre posts

2. **Upgrade para Twitter API v2**
   - Métricas completas (Views, Impressions reais)
   - Análise de sentimento

3. **Alertas e Notificações**
   - Notificar quando métricas atingem metas
   - Alertar sobre quedas de engajamento

4. **Exportação de Dados**
   - CSV, Excel
   - Relatórios PDF

5. **Multi-usuário OAuth**
   - Cada usuário com suas próprias credenciais Twitter
   - OAuth 1.0a flow completo
