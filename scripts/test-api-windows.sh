#!/bin/bash
# Script de testes para Windows/Git Bash
# Evita problemas de encoding UTF-8 criando arquivos JSON temporários

set -e

# Cores para output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Detectar ambiente
API_URL="http://localhost:5228"

# Verificar se estamos no WSL
if grep -qi microsoft /proc/version 2>/dev/null; then
    echo -e "${YELLOW}⚠️  Detectado WSL - Tentando conectar via Windows host${NC}"
    # Tentar localhost primeiro, se falhar usar IP do Windows
    if ! curl -s -o /dev/null -w "%{http_code}" --connect-timeout 2 http://localhost:5228/swagger/index.html 2>/dev/null | grep -q "200"; then
        WINDOWS_IP=$(cat /etc/resolv.conf | grep nameserver | awk '{print $2}')
        echo -e "${YELLOW}⚠️  localhost não funciona, tentando Windows IP: $WINDOWS_IP${NC}"
        API_URL="http://$WINDOWS_IP:5228"

        # Testar se funciona
        if ! curl -s -o /dev/null -w "%{http_code}" --connect-timeout 2 $API_URL/swagger/index.html 2>/dev/null | grep -q "200"; then
            echo -e "${RED}❌ Não foi possível conectar à API${NC}"
            echo -e "${BLUE}Execute este script no Git Bash do Windows em vez do WSL:${NC}"
            echo "  - Abra Git Bash (não WSL)"
            echo "  - cd /e/PROJETOS/InfluenciAI-"
            echo "  - export TOKEN=\"seu-token\""
            echo "  - bash scripts/test-api-windows.sh"
            exit 1
        fi
    fi
fi

echo -e "${BLUE}=== Teste da API InfluenciAI ===${NC}"
echo -e "${BLUE}URL: $API_URL${NC}\n"

# Verificar se TOKEN está definido
if [ -z "$TOKEN" ]; then
    echo -e "${RED}❌ Variável TOKEN não está definida!${NC}"
    echo -e "${BLUE}Execute primeiro:${NC}"
    echo 'export TOKEN="seu-token-aqui"'
    exit 1
fi

echo -e "${GREEN}✓ Token encontrado${NC}\n"

# Criar diretório temporário
TMP_DIR="/tmp/influenciai-test"
mkdir -p "$TMP_DIR"

echo -e "${BLUE}1. Criando conteúdo...${NC}"

# Criar arquivo JSON para conteúdo (evita problemas de encoding)
cat > "$TMP_DIR/create-content.json" << 'EOF'
{
  "title": "Teste de Metricas",
  "body": "Este e um tweet de teste para coleta de metricas! #InfluenciAI",
  "type": "Text"
}
EOF

# Criar conteúdo
CONTENT_RESPONSE=$(curl -s -X POST "$API_URL/api/content" \
  -H "Content-Type: application/json; charset=utf-8" \
  -H "Authorization: Bearer $TOKEN" \
  --data-binary @"$TMP_DIR/create-content.json")

# Verificar se teve erro
if echo "$CONTENT_RESPONSE" | grep -q "error\|Error\|Exception"; then
    echo -e "${RED}❌ Erro ao criar conteúdo:${NC}"
    echo "$CONTENT_RESPONSE"
    exit 1
fi

# Extrair contentId (funciona com ou sem jq)
if command -v jq &> /dev/null; then
    CONTENT_ID=$(echo "$CONTENT_RESPONSE" | jq -r '.id // .contentId // .Id')
else
    # Fallback sem jq - procura por padrão de GUID
    CONTENT_ID=$(echo "$CONTENT_RESPONSE" | grep -oP '(?<="id":"|"contentId":"|"Id":")[a-f0-9-]{36}' | head -1)
fi

if [ -z "$CONTENT_ID" ] || [ "$CONTENT_ID" == "null" ]; then
    echo -e "${RED}❌ Não foi possível extrair contentId${NC}"
    echo "Resposta: $CONTENT_RESPONSE"
    exit 1
fi

echo -e "${GREEN}✓ Conteúdo criado: $CONTENT_ID${NC}\n"

echo -e "${BLUE}2. Conectando perfil social Twitter...${NC}"

# Criar arquivo JSON para conectar perfil social
# TokenExpiresAt: 1 ano no futuro
EXPIRES_AT=$(date -u -d "+1 year" +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || date -u -v+1y +"%Y-%m-%dT%H:%M:%SZ" 2>/dev/null || echo "2026-01-01T00:00:00Z")

cat > "$TMP_DIR/create-profile.json" << EOF
{
  "network": "Twitter",
  "accessToken": "dummy_token_will_use_config",
  "refreshToken": null,
  "tokenExpiresAt": "$EXPIRES_AT"
}
EOF

# Conectar perfil social
PROFILE_RESPONSE=$(curl -s -X POST "$API_URL/api/social-profiles/connect" \
  -H "Content-Type: application/json; charset=utf-8" \
  -H "Authorization: Bearer $TOKEN" \
  --data-binary @"$TMP_DIR/create-profile.json")

# Verificar se teve erro
if echo "$PROFILE_RESPONSE" | grep -q "error\|Error\|Exception"; then
    echo -e "${RED}❌ Erro ao criar perfil social:${NC}"
    echo "$PROFILE_RESPONSE"
    exit 1
fi

# Extrair profileId
if command -v jq &> /dev/null; then
    PROFILE_ID=$(echo "$PROFILE_RESPONSE" | jq -r '.id // .profileId // .Id')
else
    PROFILE_ID=$(echo "$PROFILE_RESPONSE" | grep -oP '(?<="id":"|"profileId":"|"Id":")[a-f0-9-]{36}' | head -1)
fi

if [ -z "$PROFILE_ID" ] || [ "$PROFILE_ID" == "null" ]; then
    echo -e "${RED}❌ Não foi possível extrair profileId${NC}"
    echo "Resposta: $PROFILE_RESPONSE"
    exit 1
fi

echo -e "${GREEN}✓ Perfil criado: $PROFILE_ID${NC}\n"

echo -e "${BLUE}3. Publicando no Twitter...${NC}"

# Criar arquivo JSON para publicação
cat > "$TMP_DIR/publish.json" << EOF
{
  "socialProfileId": "$PROFILE_ID"
}
EOF

# Publicar
PUBLISH_RESPONSE=$(curl -s -X POST "$API_URL/api/content/$CONTENT_ID/publish" \
  -H "Content-Type: application/json; charset=utf-8" \
  -H "Authorization: Bearer $TOKEN" \
  --data-binary @"$TMP_DIR/publish.json")

# Verificar se teve erro
if echo "$PUBLISH_RESPONSE" | grep -q "error\|Error\|Exception"; then
    echo -e "${RED}❌ Erro ao publicar:${NC}"
    echo "$PUBLISH_RESPONSE"
    exit 1
fi

# Extrair informações da publicação
if command -v jq &> /dev/null; then
    PUBLICATION_ID=$(echo "$PUBLISH_RESPONSE" | jq -r '.publicationId // .id // .Id')
    TWEET_ID=$(echo "$PUBLISH_RESPONSE" | jq -r '.tweetId // .externalId // ""')
    TWEET_URL=$(echo "$PUBLISH_RESPONSE" | jq -r '.tweetUrl // .url // ""')
else
    PUBLICATION_ID=$(echo "$PUBLISH_RESPONSE" | grep -oP '(?<="publicationId":"|"id":")[a-f0-9-]{36}' | head -1)
    TWEET_ID=$(echo "$PUBLISH_RESPONSE" | grep -oP '(?<="tweetId":"|"externalId":")[0-9]+"' | head -1)
fi

echo -e "${GREEN}✓ Publicado com sucesso!${NC}"
echo -e "  Publication ID: $PUBLICATION_ID"
[ ! -z "$TWEET_ID" ] && echo -e "  Tweet ID: $TWEET_ID"
[ ! -z "$TWEET_URL" ] && echo -e "  URL: $TWEET_URL"
echo ""

echo -e "${BLUE}4. Consultando métricas iniciais...${NC}"

# Consultar métricas
METRICS_RESPONSE=$(curl -s -X GET "$API_URL/api/content/$CONTENT_ID/metrics" \
  -H "Authorization: Bearer $TOKEN")

if command -v jq &> /dev/null; then
    echo "$METRICS_RESPONSE" | jq '.'
else
    echo "$METRICS_RESPONSE"
fi

echo ""
echo -e "${GREEN}=== ✓ Teste completo ===${NC}\n"
echo -e "${BLUE}IDs para usar em testes futuros:${NC}"
echo "export CONTENT_ID=\"$CONTENT_ID\""
echo "export PROFILE_ID=\"$PROFILE_ID\""
[ ! -z "$PUBLICATION_ID" ] && echo "export PUBLICATION_ID=\"$PUBLICATION_ID\""

echo ""
echo -e "${BLUE}Para acompanhar a coleta automática de métricas:${NC}"
echo "curl -s -X GET \"$API_URL/api/content/$CONTENT_ID/metrics\" -H \"Authorization: Bearer \$TOKEN\""

# Limpar arquivos temporários
rm -rf "$TMP_DIR"
