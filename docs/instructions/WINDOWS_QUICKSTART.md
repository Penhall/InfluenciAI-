# 🪟 Guia Rápido para Windows

Este guia resolve problemas comuns ao testar a API no Windows/Git Bash.

## ⚡ Solução Rápida - Use o Script

```bash
# 1. Fazer login e pegar o token
curl -X POST http://localhost:5228/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin!234"}'

# 2. Exportar o token
export TOKEN="seu-token-aqui"

# 3. Rodar o script de teste
./scripts/test-api-windows.sh
```

O script `test-api-windows.sh` resolve automaticamente os problemas de encoding UTF-8 do Git Bash.

## 🐛 Problema: Erro de UTF-8

**Erro que você vê:**
```
System.Text.DecoderFallbackException: Unable to translate bytes [E9] at index...
```

**Causa:** Git Bash no Windows envia caracteres acentuados em ISO-8859-1 em vez de UTF-8.

### Solução 1: Use o Script (Recomendado)

```bash
./scripts/test-api-windows.sh
```

### Solução 2: Use Arquivos JSON

Em vez de passar JSON inline no curl, crie arquivos:

```bash
# Criar arquivo
cat > /tmp/test.json << 'EOF'
{
  "title": "Test Metrics",
  "body": "This is a test tweet #InfluenciAI",
  "status": "draft"
}
EOF

# Usar arquivo
curl -X POST http://localhost:5228/api/content \
  -H "Content-Type: application/json; charset=utf-8" \
  -H "Authorization: Bearer $TOKEN" \
  --data-binary @/tmp/test.json
```

### Solução 3: Evite Acentos

Use apenas ASCII no JSON inline:

```bash
curl -X POST http://localhost:5228/api/content \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"title":"Test Metrics","body":"This is a test tweet #InfluenciAI","status":"draft"}'
```

### Solução 4: Use WSL Diretamente (Melhor para Desenvolvimento)

Em vez de usar Git Bash, use WSL:

```bash
# No PowerShell ou CMD
wsl

# Agora você está no WSL
cd /mnt/e/PROJETOS/InfluenciAI-

# Os comandos curl funcionarão corretamente com UTF-8
curl -X POST http://localhost:5228/api/content \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Teste de Métricas",
    "body": "Este é um tweet de teste! #InfluenciAI",
    "status": "draft"
  }'
```

## 📋 Fluxo Completo de Teste

### 1. Iniciar Ambiente

```bash
# No WSL (recomendado) ou Git Bash
./scripts/start-dev.sh

# Em outro terminal, iniciar a API
cd src/Server/InfluenciAI.Api
dotnet run
```

### 2. Login

```bash
curl -X POST http://localhost:5228/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin!234"}'
```

Copie o `access_token` da resposta.

### 3. Executar Testes

```bash
# Exportar token
export TOKEN="cole-seu-token-aqui"

# Rodar script de teste
./scripts/test-api-windows.sh
```

O script criará:
- ✅ Conteúdo
- ✅ Perfil social Twitter
- ✅ Publicação no Twitter
- ✅ Consulta de métricas

### 4. Acompanhar Coleta Automática

```bash
# O script mostrará os IDs criados
export CONTENT_ID="id-mostrado-pelo-script"

# Consultar métricas (repita a cada 5 minutos)
curl -s -X GET http://localhost:5228/api/content/$CONTENT_ID/metrics \
  -H "Authorization: Bearer $TOKEN"
```

## 🔍 Verificar se Está Funcionando

### API Rodando?

```bash
curl -s http://localhost:5228/swagger/index.html
# Se retornar HTML, está funcionando
```

### Docker Rodando?

```bash
docker ps
# Deve mostrar: postgres, redis, rabbitmq
```

### Métricas Sendo Coletadas?

Consulte os logs da API e procure por:

```
[INF] DataCollectorService started
[INF] Found X recent publications to collect metrics for
[INF] Collecting metrics for publication {...}
```

## 🆘 Problemas Comuns

### "command not found: dotnet"

No WSL, instale o .NET:

```bash
./scripts/dotnet-install.sh
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

### "docker: command not found"

1. Instale Docker Desktop para Windows
2. Nas configurações, habilite "WSL 2 based engine"
3. Em Resources > WSL Integration, habilite sua distro
4. Reinicie o terminal WSL

### "Cannot connect to PostgreSQL"

```bash
# Verificar se está rodando
docker ps | grep postgres

# Reiniciar se necessário
docker-compose restart postgres

# Aguardar 10 segundos
sleep 10

# Rodar migrations
cd src/Server/InfluenciAI.Api
dotnet ef database update
```

## 💡 Dicas

### Use WSL em vez de Git Bash

WSL tem melhor compatibilidade com ferramentas Linux e não tem problemas de encoding UTF-8.

### Instale jq para Formatar JSON

```bash
# No WSL
sudo apt-get install jq

# Usar com curl
curl -s ... | jq '.'
```

### Use watch para Monitorar Métricas

```bash
# Instalar watch (WSL)
sudo apt-get install procps

# Monitorar métricas a cada 30 segundos
watch -n 30 'curl -s http://localhost:5228/api/content/$CONTENT_ID/metrics -H "Authorization: Bearer $TOKEN" | jq ".timeseriesMetrics | length"'
```

## 📚 Mais Informações

Para documentação completa, veja:
- `TESTING_GUIDE.md` - Guia completo de testes
- `TROUBLESHOOTING_LOGIN.md` - Problemas de autenticação
- `FIX_LOGIN_VISUAL_STUDIO.md` - Configuração do Visual Studio
