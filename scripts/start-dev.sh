#!/bin/bash

# Script para iniciar o ambiente de desenvolvimento completo

set -e

echo "🚀 InfluenciAI - Iniciando ambiente de desenvolvimento"
echo ""

# Cores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Navegar para o diretório raiz do projeto
cd "$(dirname "$0")/.."

# 1. Iniciar containers Docker
echo -e "${BLUE}📦 Iniciando containers Docker...${NC}"
docker-compose up -d

# 2. Aguardar PostgreSQL estar pronto
echo -e "${BLUE}⏳ Aguardando PostgreSQL inicializar...${NC}"
for i in {1..30}; do
    if docker exec influenciai_postgres pg_isready -U postgres > /dev/null 2>&1; then
        echo -e "${GREEN}✅ PostgreSQL pronto!${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${YELLOW}⚠️  PostgreSQL demorou para inicializar${NC}"
        exit 1
    fi
    echo -n "."
    sleep 1
done
echo ""

# 3. Verificar status dos containers
echo -e "${BLUE}🐳 Status dos containers:${NC}"
docker-compose ps

echo ""

# 4. Aplicar migrations
echo -e "${BLUE}🗄️  Aplicando migrations ao banco de dados...${NC}"
export DOTNET_ROOT=$HOME/.dotnet
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

dotnet ef database update \
    --project src/Infra/InfluenciAI.Infrastructure/InfluenciAI.Infrastructure.csproj \
    --startup-project src/Server/InfluenciAI.Api/InfluenciAI.Api.csproj

echo ""
echo -e "${GREEN}✅ Ambiente pronto!${NC}"
echo ""
echo "📝 Serviços disponíveis:"
echo "   🐘 PostgreSQL: localhost:5432"
echo "      Database: influenciai"
echo "      User: postgres"
echo "      Password: postgres"
echo ""
echo "   🔴 Redis: localhost:6379"
echo ""
echo "   🐰 RabbitMQ: localhost:5672"
echo "      Management UI: http://localhost:15672"
echo "      User: guest / Password: guest"
echo ""
echo "🚀 Para iniciar a API, execute:"
echo "   cd src/Server/InfluenciAI.Api && dotnet run"
echo ""
