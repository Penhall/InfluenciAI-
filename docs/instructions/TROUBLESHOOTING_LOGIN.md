# Troubleshooting - Problema de Login

## 🔍 Diagnóstico Realizado

✅ **Banco de Dados:** PostgreSQL está rodando
✅ **Usuário Admin:** Existe no banco (`admin@local`)
❌ **Login:** Retorna "Não autorizado"

## 🎯 Possíveis Causas

### 1. API Rodando em Porta Diferente

Quando você roda pelo Visual Studio, a porta pode ser diferente de `5228`.

**Como verificar:**
- No Visual Studio, olhe a aba "Output" quando a API inicia
- Procure por: `Now listening on: http://localhost:XXXX`
- Use essa porta nos testes

**Exemplo:**
```
Now listening on: http://localhost:60790
```

Então use: `http://localhost:60790/auth/login`

### 2. Senha do Admin Pode Estar Diferente

A senha esperada é: `Admin!234`

Se mudou, precisamos resetar.

## 🔧 Soluções

### Solução 1: Descobrir a Porta Correta

1. **No Visual Studio:**
   - Inicie a API (F5)
   - Vá em `View` → `Output`
   - Selecione "InfluenciAI.Api" no dropdown
   - Procure por "Now listening on"

2. **Teste com a porta correta:**
   ```bash
   curl -X POST http://localhost:PORTA_CORRETA/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@local","password":"Admin!234"}'
   ```

### Solução 2: Verificar se Está Usando PostgreSQL

1. **Nos logs da API, procure por:**
   ```
   ✅ [INF] Seed completed. Tenant=Default, Admin=admin@local
   ```

   OU

   ```
   ❌ [WRN] Using InMemory database - skipping migrations
   ```

2. **Se estiver usando InMemory:**
   - A API não está lendo o `appsettings.Development.json`
   - Verifique se o arquivo existe em: `src/Server/InfluenciAI.Api/appsettings.Development.json`
   - Verifique se a ConnectionString está correta

### Solução 3: Resetar Senha do Admin

Execute este comando SQL no PostgreSQL:

```sql
-- Conectar ao banco
docker exec influenciai_postgres psql -U postgres -d influenciai

-- Resetar o usuário admin (deleta e recria)
DELETE FROM "AspNetUsers" WHERE "Email" = 'admin@local';
```

Depois, reinicie a API para o seed rodar novamente.

### Solução 4: Testar Conexão Direta com Banco

```bash
# Verificar se o usuário existe
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'SELECT "Email", "UserName" FROM "AspNetUsers";'

# Verificar se há tenants
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'SELECT "Id", "Name" FROM "Tenants";'
```

## 📝 Checklist de Diagnóstico

Execute estes passos e me informe os resultados:

- [ ] **Passo 1:** Qual porta a API está usando?
  ```
  Verifique no Output do Visual Studio: "Now listening on: http://localhost:____"
  ```

- [ ] **Passo 2:** A API está usando PostgreSQL?
  ```
  Procure nos logs por: "[INF] Seed completed" ou "[WRN] Using InMemory"
  ```

- [ ] **Passo 3:** Teste de login com a porta correta
  ```bash
  curl -X POST http://localhost:PORTA/auth/login \
    -H "Content-Type: application/json" \
    -d '{"email":"admin@local","password":"Admin!234"}'
  ```

- [ ] **Passo 4:** Qual o erro exato?
  ```
  Copie a resposta completa do curl acima
  ```

## 🐛 Comandos de Debug

### Ver logs da API em tempo real (se rodando via CLI)
```bash
cd src/Server/InfluenciAI.Api
dotnet run
# Logs aparecerão aqui
```

### Testar conexão com PostgreSQL
```bash
docker exec influenciai_postgres pg_isready -U postgres
# Deve retornar: postgres:5432 - accepting connections
```

### Ver todas as portas em uso
```bash
netstat -ano | findstr LISTENING  # Windows
# ou
ss -tlnp  # Linux
```

## 💡 Dica Rápida

Se você estiver usando o Visual Studio e o IIS Express, a porta pode ser a `60790` (conforme launchSettings.json).

Tente:
```bash
curl -X POST http://localhost:60790/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin!234"}'
```

## 📞 Informações Necessárias

Para eu ajudar melhor, me envie:

1. **Porta que a API está usando** (do Output do Visual Studio)
2. **Se aparece "Seed completed" ou "Using InMemory"** nos logs
3. **Resposta completa do curl** quando tenta fazer login
4. **Como está rodando a API:** Visual Studio (F5) ou CLI (dotnet run)

---

## 🔐 Credenciais de Teste (Padrão)

- **Email:** `admin@local`
- **Senha:** `Admin!234`
- **Tenant:** `Default`

Se essas credenciais não funcionarem, precisamos resetar o banco ou criar um novo usuário.
