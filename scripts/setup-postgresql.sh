#!/bin/bash

# Script para configurar PostgreSQL no Ubuntu/Debian (WSL)

echo "🐘 Configurando PostgreSQL para InfluenciAI..."

# Atualizar pacotes
echo "📦 Atualizando lista de pacotes..."
sudo apt update

# Instalar PostgreSQL
echo "⬇️  Instalando PostgreSQL..."
sudo apt install -y postgresql postgresql-contrib

# Iniciar serviço
echo "▶️  Iniciando serviço PostgreSQL..."
sudo service postgresql start

# Configurar usuário postgres
echo "🔐 Configurando usuário postgres..."
sudo -u postgres psql -c "ALTER USER postgres PASSWORD 'postgres';"

# Criar banco de dados
echo "🗄️  Criando banco de dados influenciai..."
sudo -u postgres createdb influenciai || echo "Banco já existe"

# Testar conexão
echo "✅ Testando conexão..."
PGPASSWORD=postgres psql -U postgres -h localhost -d influenciai -c "SELECT version();"

echo ""
echo "✅ PostgreSQL configurado com sucesso!"
echo ""
echo "📝 Credenciais:"
echo "   Host: localhost"
echo "   Port: 5432"
echo "   Database: influenciai"
echo "   Username: postgres"
echo "   Password: postgres"
echo ""
echo "🚀 Próximos passos:"
echo "   1. Aplicar migrations: dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api"
echo "   2. Iniciar a API: cd src/Server/InfluenciAI.Api && dotnet run"
