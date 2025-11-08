#!/bin/bash

# Script para reiniciar API e forçar seed do usuário admin

echo "🔄 Reiniciando API e executando seed..."
echo ""

# 1. Parar processos dotnet
echo "🛑 Parando processos da API..."
pkill -f "dotnet.*InfluenciAI.Api" || true
sleep 2

# 2. Limpar usuários existentes (para forçar seed)
echo "🗑️  Limpando usuários..."
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'DELETE FROM "AspNetUsers";' 2>/dev/null
docker exec influenciai_postgres psql -U postgres -d influenciai -c 'DELETE FROM "Tenants";' 2>/dev/null

# 3. Verificar que está limpo
USER_COUNT=$(docker exec influenciai_postgres psql -U postgres -d influenciai -t -c 'SELECT COUNT(*) FROM "AspNetUsers";' 2>/dev/null | tr -d ' \r\n')
echo "Usuários no banco: $USER_COUNT"

# 4. Iniciar API
echo ""
echo "▶️  Iniciando API..."
echo "A API vai:"
echo "  1. Conectar ao PostgreSQL"
echo "  2. Detectar que não há admin"
echo "  3. Criar usuário: admin@local"
echo "  4. Criar senha: Admin123!"
echo ""
echo "Aguarde 10 segundos..."

cd src/Server/InfluenciAI.Api
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
export ASPNETCORE_ENVIRONMENT=Development

dotnet run &
API_PID=$!

echo "API iniciada (PID: $API_PID)"
echo ""

# 5. Aguardar seed
sleep 15

# 6. Verificar se o admin foi criado
echo "🔍 Verificando se o admin foi criado..."
USER_COUNT=$(docker exec influenciai_postgres psql -U postgres -d influenciai -t -c 'SELECT COUNT(*) FROM "AspNetUsers";' 2>/dev/null | tr -d ' \r\n')

if [ "$USER_COUNT" == "1" ]; then
    echo "✅ SUCESSO! Usuário admin criado!"

    # Mostrar detalhes
    docker exec influenciai_postgres psql -U postgres -d influenciai -c 'SELECT "Email", "UserName" FROM "AspNetUsers";'

    echo ""
    echo "🎯 Credenciais:"
    echo "   Email: admin@local"
    echo "   Senha: Admin123!"
    echo ""
    echo "📝 Tente fazer login agora!"
    echo ""
    echo "🌐 Se rodando pelo Visual Studio, use a porta que aparece no Output"
    echo "🌐 Se rodando por este script, a porta padrão é 5228"
else
    echo "❌ ERRO: Admin não foi criado"
    echo "   Usuários no banco: $USER_COUNT"
    echo ""
    echo "Verifique os logs da API acima para ver o erro"
fi

echo ""
echo "API está rodando em background (PID: $API_PID)"
echo "Para ver logs: tail -f nohup.out"
echo "Para parar: kill $API_PID"
