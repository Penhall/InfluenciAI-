#!/bin/bash

# Script para resetar a senha do admin
# Remove o usuário admin existente e força o seed a recriar

echo "🔐 Reset de Senha do Admin"
echo ""

# Verificar se o PostgreSQL está rodando
if ! docker ps | grep -q influenciai_postgres; then
    echo "❌ PostgreSQL não está rodando!"
    echo "Execute: docker-compose up -d"
    exit 1
fi

echo "✅ PostgreSQL está rodando"
echo ""

# Remover usuário admin existente
echo "🗑️  Removendo usuário admin existente..."
docker exec influenciai_postgres psql -U postgres -d influenciai -c "DELETE FROM \"AspNetUsers\" WHERE \"Email\" = 'admin@local';" 2>/dev/null

echo "✅ Usuário removido"
echo ""

# Verificar se foi removido
USER_COUNT=$(docker exec influenciai_postgres psql -U postgres -d influenciai -t -c "SELECT COUNT(*) FROM \"AspNetUsers\" WHERE \"Email\" = 'admin@local';" 2>/dev/null | tr -d ' ')

if [ "$USER_COUNT" == "0" ]; then
    echo "✅ Confirmado: Usuário admin foi removido"
else
    echo "⚠️  Aviso: Usuário admin ainda existe"
fi

echo ""
echo "📝 Próximos passos:"
echo "1. Reinicie a API (ela vai detectar que não há admin e criar um novo)"
echo "2. A nova senha será: Admin123!"
echo "3. Teste o login com: email=admin@local, password=Admin123!"
echo ""
echo "Para reiniciar a API:"
echo "  - Visual Studio: Pare (Shift+F5) e Inicie (F5) novamente"
echo "  - CLI: Ctrl+C e rode 'dotnet run' novamente"
echo ""
