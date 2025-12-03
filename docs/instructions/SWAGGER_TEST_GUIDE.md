# 🧪 Guia de Teste via Swagger UI

Este guia mostra como testar todos os endpoints usando a interface do Swagger.

## 1️⃣ Abrir o Swagger

Com a API rodando no Visual Studio, acesse:
```
http://localhost:5228/swagger
```

## 2️⃣ Fazer Login

1. Encontre o endpoint **`POST /auth/login`**
2. Clique em **"Try it out"**
3. Cole o seguinte JSON no corpo da requisição:

```json
{
  "email": "admin@local",
  "password": "Admin!234"
}
```

4. Clique em **"Execute"**
5. Na resposta, **copie o valor do campo `access_token`**

## 3️⃣ Autenticar no Swagger

1. No topo da página do Swagger, clique no botão **"Authorize"** 🔓
2. No campo "Value", cole o token no formato:
   ```
   Bearer seu-token-aqui
   ```
   **Exemplo:**
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. Clique em **"Authorize"**
4. Clique em **"Close"**

Agora todos os endpoints autenticados funcionarão! 🎉

## 4️⃣ Criar Conteúdo

1. Encontre **`POST /api/content`**
2. Clique em **"Try it out"**
3. Cole o JSON:

```json
{
  "title": "Teste de Metricas",
  "body": "Este e um tweet de teste para coleta de metricas! #InfluenciAI",
  "type": "Text"
}
```

**Nota:** Os valores válidos para `type` são:
- `"Text"` - Conteúdo de texto (tweets, posts)
- `"Image"` - Imagem
- `"Video"` - Vídeo
- `"Link"` - Link compartilhado

4. Clique em **"Execute"**
5. **Copie o `id` da resposta** - você precisará dele!

## 5️⃣ Conectar Perfil Social Twitter

1. Encontre **`POST /api/social-profiles/connect`**
2. Clique em **"Try it out"**
3. Cole o JSON:

```json
{
  "network": "Twitter",
  "accessToken": "dummy_token_will_use_config",
  "refreshToken": null,
  "tokenExpiresAt": "2026-01-01T00:00:00Z"
}
```

4. Clique em **"Execute"**
5. **Copie o `id` da resposta** - você precisará dele!

## 6️⃣ Publicar no Twitter

1. Encontre **`POST /api/content/{contentId}/publish`**
2. Clique em **"Try it out"**
3. No campo **`contentId`**, cole o ID do conteúdo que você criou
4. No corpo, cole:

```json
{
  "socialProfileId": "cole-o-id-do-perfil-aqui"
}
```

5. Clique em **"Execute"**
6. Se tudo der certo, você verá na resposta:
   - `publicationId`
   - `tweetId` (ID real do tweet no Twitter)
   - `tweetUrl` (link para o tweet)

## 7️⃣ Consultar Métricas

1. Encontre **`GET /api/content/{contentId}/metrics`**
2. Clique em **"Try it out"**
3. No campo **`contentId`**, cole o ID do conteúdo
4. Clique em **"Execute"**
5. Você verá:

```json
{
  "contentId": "...",
  "title": "Teste de Metricas",
  "body": "Este e um tweet...",
  "publishedAt": "2025-11-13T...",
  "latestMetrics": {
    "id": "...",
    "publicationId": "...",
    "collectedAt": "2025-11-13T...",
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

## 8️⃣ Acompanhar Coleta Automática

O sistema coleta métricas automaticamente:
- **0-2 horas:** A cada 5 minutos
- **2-6 horas:** A cada 30 minutos
- **6-24 horas:** A cada 1 hora

Para ver a evolução:
1. Aguarde alguns minutos
2. Execute novamente **`GET /api/content/{contentId}/metrics`**
3. O array `timeseriesMetrics` terá mais entradas

## 🔍 Outros Endpoints Úteis

### Listar Conteúdos
**`GET /api/content`** - Lista todos os conteúdos criados

### Listar Perfis Sociais
**`GET /api/social-profiles`** - Lista todos os perfis conectados

### Atualizar Token
**`POST /auth/refresh`** - Renova o token expirado
```json
{
  "refresh_token": "seu-refresh-token"
}
```

## 🐛 Troubleshooting

### "401 Unauthorized"
- Você não autenticou no Swagger
- Clique no botão **"Authorize"** 🔓 no topo
- Cole o token com o prefixo `Bearer `

### "404 Not Found" no endpoint
- Verifique se está usando o endpoint correto:
  - ✅ `/api/social-profiles/connect` (correto)
  - ❌ `/api/social-profiles` POST (não existe)

### "Invalid network" ou erro de enum
- O campo `network` precisa ser exatamente: `"Twitter"` (com T maiúsculo)
- O campo `type` precisa ser um dos seguintes (com primeira letra maiúscula):
  - `"Text"` - Para tweets/posts de texto
  - `"Image"` - Para imagens
  - `"Video"` - Para vídeos
  - `"Link"` - Para links compartilhados

### "Token expirado"
- Tokens duram 8 horas
- Faça login novamente para pegar um novo token
- Ou use o endpoint `/auth/refresh` com o `refresh_token`

## 💡 Dicas

### Ver Logs em Tempo Real
No Visual Studio, abra a janela **Output** e selecione "Show output from: InfluenciAI.Api"

Procure por:
```
[INF] DataCollectorService started
[INF] Found X recent publications to collect metrics for
[INF] Collecting metrics for publication {...}
[INF] Saved metrics snapshot: Likes=X, Retweets=Y
```

### Copiar IDs Facilmente
No Swagger, após executar uma requisição:
1. Clique em **"Download"** ao lado da resposta
2. Ou simplesmente copie o JSON da resposta

### Testar com Postman/Insomnia
Se preferir, use o Postman ou Insomnia:
1. Crie uma collection
2. Adicione um header `Authorization: Bearer seu-token`
3. Faça as requisições normalmente

## 📊 Exemplo de Fluxo Completo

```
1. POST /auth/login → pegar token
2. Authorize no Swagger com "Bearer {token}"
3. POST /api/content → criar conteúdo (copiar contentId)
4. POST /api/social-profiles/connect → conectar Twitter (copiar profileId)
5. POST /api/content/{contentId}/publish → publicar no Twitter
6. GET /api/content/{contentId}/metrics → ver métricas iniciais
7. Aguardar 5 minutos
8. GET /api/content/{contentId}/metrics → ver métricas atualizadas
```

Pronto! Agora você pode testar todo o fluxo facilmente via Swagger! 🚀
