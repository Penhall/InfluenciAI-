#!/bin/bash

# Script para testar login em diferentes portas

echo "🔍 Testando Login na API InfluenciAI"
echo ""

PORTS=(5228 60790 7000 5000 8080)
EMAIL="admin@local"
PASSWORD="Admin123!"

for PORT in "${PORTS[@]}"; do
    echo "Testando porta $PORT..."

    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST http://localhost:$PORT/auth/login \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}" 2>/dev/null)

    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    BODY=$(echo "$RESPONSE" | head -n-1)

    if [ "$HTTP_CODE" == "200" ]; then
        echo "✅ SUCESSO na porta $PORT!"
        echo "   Token recebido:"
        echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
        echo ""
        echo "🎯 Use esta porta: http://localhost:$PORT"
        exit 0
    elif [ "$HTTP_CODE" == "401" ]; then
        echo "❌ Porta $PORT: Não autorizado (senha incorreta ou usuário não existe)"
    elif [ "$HTTP_CODE" == "000" ]; then
        echo "⚪ Porta $PORT: Não está respondendo (API não está nesta porta)"
    else
        echo "⚠️  Porta $PORT: HTTP $HTTP_CODE"
    fi
    echo ""
done

echo "❌ Nenhuma porta funcionou!"
echo ""
echo "📝 Possíveis soluções:"
echo "1. Verifique se a API está rodando"
echo "2. Verifique a porta no Output do Visual Studio"
echo "3. Execute: ./scripts/reset-admin-password.sh"
echo ""
