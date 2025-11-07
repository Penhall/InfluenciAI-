#!/bin/bash

# Script para parar o ambiente de desenvolvimento

cd "$(dirname "$0")/.."

echo "🛑 Parando containers Docker..."
docker-compose down

echo "✅ Containers parados!"
echo ""
echo "💡 Para iniciar novamente, execute: ./scripts/start-dev.sh"
