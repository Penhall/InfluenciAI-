# 🔧 Como Resolver o Problema de Login no Visual Studio

## 🚨 O PROBLEMA

O banco de dados está **VAZIO** - não há usuário admin!
Por isso o login retorna "Não autorizado".

## ✅ SOLUÇÃO RÁPIDA (Visual Studio)

### **PASSO 1: Parar a API**

No Visual Studio:
- Pressione `Shift + F5` (ou clique no botão Stop)
- Aguarde a API parar completamente

### **PASSO 2: Limpar o Banco (IMPORTANTE!)**

Abra o terminal (PowerShell ou CMD) e execute:

```powershell
# PowerShell
docker exec influenciai_postgres psql -U postgres -d influenciai -c "DELETE FROM \"AspNetUsers\";"
docker exec influenciai_postgres psql -U postgres -d influenciai -c "DELETE FROM \"Tenants\";"
```

Ou no WSL/Linux:
```bash
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'DELETE FROM "AspNetUsers";'
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'DELETE FROM "Tenants";'
```

Você deve ver:
```
DELETE 0
DELETE 0
```

### **PASSO 3: Iniciar a API Novamente**

No Visual Studio:
- Pressione `F5` (ou clique em Start)
- **IMPORTANTE:** Vá em `View` → `Output`
- No dropdown, selecione "InfluenciAI.Api" ou "Debug"

### **PASSO 4: Verificar os Logs**

Procure por estas mensagens no Output:

✅ **SUCESSO** - Você deve ver:
```
[INF] Seed completed. Tenant=Default, Admin=admin@local
```

❌ **PROBLEMA** - Se você ver:
```
[WRN] Using InMemory database - skipping migrations
```

Significa que a API não está lendo o `appsettings.Development.json`!

### **PASSO 5: Descobrir a Porta**

No Output, procure por:
```
Now listening on: http://localhost:XXXXX
```

Anote esse número (exemplo: 5228, 60790, etc)

### **PASSO 6: Testar o Login**

Abra outro terminal e teste:

```bash
# Substitua XXXXX pela porta que você anotou
curl -X POST http://localhost:XXXXX/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin!234"}'
```

Se funcionar, você vai ver um JSON com `"access_token"` e `"refresh_token"` 🎉

---

## 🔧 SE AINDA NÃO FUNCIONAR

### Problema 1: "Using InMemory database"

A API não está usando o `appsettings.Development.json`.

**Solução:**

1. Verifique se o arquivo existe: `src/Server/InfluenciAI.Api/appsettings.Development.json`
2. Abra o arquivo e confirme que tem:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=influenciai;Username=postgres;Password=postgres"
   }
   ```
3. No Visual Studio, clique com botão direito em `appsettings.Development.json`
4. Vá em `Properties`
5. Certifique-se que `Copy to Output Directory` está como `Copy if newer`
6. Rebuild (Ctrl+Shift+B) e tente novamente

### Problema 2: "Seed completed" mas login falha

O seed pode ter falhado silenciosamente.

**Solução:**

Verifique se o usuário foi criado:
```bash
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'SELECT "Email" FROM "AspNetUsers";'
```

Se retornar vazio, execute o script de seed manual:
```bash
bash scripts/restart-and-seed.sh
```

### Problema 3: Desktop também não funciona

Se o desktop WPF também parou de funcionar, provavelmente é porque:
- A API está em uma porta diferente
- O usuário admin não existe

**Solução:**

1. No desktop, verifique qual URL ele está tentando conectar
2. Atualize para a porta correta (a que aparece no Output do Visual Studio)
3. Execute os PASSOS 1-6 acima para recriar o admin

---

## 📋 CHECKLIST RÁPIDO

- [ ] API parada (Shift+F5)
- [ ] Banco limpo (comando DELETE executado)
- [ ] API iniciada (F5)
- [ ] Output mostra "Seed completed"
- [ ] Porta anotada do Output
- [ ] Login testado com curl
- [ ] Desktop atualizado com porta correta (se aplicável)

---

## 🆘 ÚLTIMO RECURSO

Se nada funcionar, execute este script que faz tudo automaticamente:

```bash
bash scripts/restart-and-seed.sh
```

Aguarde 15 segundos e teste o login na porta 5228:

```bash
curl -X POST http://localhost:5228/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@local","password":"Admin!234"}'
```

---

## 📞 INFORMAÇÕES PARA DEBUG

Se ainda não funcionar, me envie:

1. **Output completo da API** (copie e cole o Output do Visual Studio)
2. **Resultado do comando:**
   ```bash
   docker exec influenciai_postgres psql -U postgres -d influenciai -c 'SELECT COUNT(*) FROM "AspNetUsers";'
   ```
3. **Porta que a API está usando**
4. **Resposta exata do curl de login**

Com essas informações, posso identificar o problema exato!

---

## 🎯 CREDENCIAIS CORRETAS

- **Email:** `admin@local`
- **Senha:** `Admin!234`
- **Tenant:** `Default` (criado automaticamente)

**ATENÇÃO:** A senha tem que ser exatamente `Admin!234` (com A maiúsculo, números e exclamação)
