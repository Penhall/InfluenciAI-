# 🚀 Guia Rápido de Teste - 3 Passos

## Passo 1: Iniciar a API no Visual Studio

1. Abra a solution no Visual Studio 2022
2. Defina `InfluenciAI.Api` como projeto de inicialização (botão direito > Set as Startup Project)
3. Pressione **F5** ou clique em **▶️ https** para iniciar
4. Aguarde o Swagger abrir em: `http://localhost:5228/swagger`

**⚠️ Importante:** A API DEVE estar rodando antes de executar os testes!

## Passo 2: Fazer Login e Pegar o Token

No **Git Bash** do Windows:

```bash
# Fazer login
curl -X POST http://localhost:5228/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin!234"}'
```

**Copie o token da resposta** (o campo `access_token`).

## Passo 3: Executar o Script de Teste

**⚠️ IMPORTANTE: Use o Git Bash do Windows (não WSL)!**

No **Git Bash** (MinGW64):

```bash
# Exportar o token (cole o seu token aqui)
export TOKEN="cole-seu-token-aqui"

# Executar o script de teste
bash scripts/test-api-windows.sh
```

**Por que Git Bash e não WSL?**
- A API está rodando no Windows (Visual Studio)
- WSL tem problemas de rede para conectar em localhost do Windows
- Git Bash compartilha a rede do Windows e funciona perfeitamente

O script irá:
- ✅ Criar um conteúdo de teste
- ✅ Criar um perfil social Twitter
- ✅ Publicar o conteúdo no Twitter
- ✅ Consultar as métricas iniciais
- ✅ Mostrar os IDs para você acompanhar a coleta automática

## 🎯 Resultado Esperado

```
=== Teste da API InfluenciAI ===

✓ Token encontrado

1. Criando conteúdo...
✓ Conteúdo criado: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

2. Criando perfil social Twitter...
✓ Perfil criado: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

3. Publicando no Twitter...
✓ Publicado com sucesso!
  Publication ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
  Tweet ID: 1234567890
  URL: https://twitter.com/user/status/1234567890

4. Consultando métricas iniciais...
{
  "contentId": "...",
  "latestMetrics": {
    "views": 0,
    "likes": 0,
    "shares": 0,
    ...
  }
}

=== ✓ Teste completo ===
```

## ⏱️ Acompanhar Coleta Automática

Após o teste, as métricas serão coletadas automaticamente:
- **Primeiras 2 horas:** A cada 5 minutos
- **2-6 horas:** A cada 30 minutos
- **6-24 horas:** A cada 1 hora

Para ver a evolução:

```bash
# Use o CONTENT_ID que o script mostrou
export CONTENT_ID="cole-o-id-aqui"

# Consultar métricas
curl -s -X GET http://localhost:5228/api/content/$CONTENT_ID/metrics \
  -H "Authorization: Bearer $TOKEN"
```

## 🐛 Problemas Comuns

### "API não responde"
- Verifique se a API está realmente rodando no Visual Studio
- Acesse manualmente: http://localhost:5228/swagger
- Se não abrir, pressione F5 no Visual Studio

### "Erro de encoding UTF-8"
- Isso é normal se você rodar curl com JSON inline acentuado no Git Bash
- O script resolve esse problema automaticamente usando arquivos temporários

### "Token expirado"
- Faça login novamente para pegar um novo token
- Tokens duram 24 horas

### "Docker não está rodando"
- Execute: `docker-compose up -d` na raiz do projeto
- Ou use: `./scripts/start-dev.sh`

## 📋 Verificar Logs da Coleta

No Visual Studio, procure nos logs por:
```
[INF] DataCollectorService started
[INF] Found X recent publications to collect metrics for
[INF] Collecting metrics for publication {...}
[INF] Saved metrics snapshot: Likes=X, Retweets=Y
```

## 💡 Dica Pro

Para não precisar ficar exportando o TOKEN toda vez, salve em um arquivo:

```bash
# Salvar token
echo "export TOKEN=\"seu-token-aqui\"" > ~/.influenciai_token
chmod 600 ~/.influenciai_token

# Carregar token
source ~/.influenciai_token

# Agora pode rodar o script sem exportar novamente
bash scripts/test-api-windows.sh
```
