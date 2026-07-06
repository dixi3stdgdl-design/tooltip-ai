#!/bin/bash
# ============================================================
# Tooltip AI — Security Audit Script
# ============================================================
# Busca referencias no autorizadas a marcas de terceros
# Uso: ./scripts/security-audit.sh

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — Auditoria de Seguridad               ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ISSUES=0

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

check_pattern() {
    local pattern="$1"
    local description="$2"
    local files="$3"
    
    echo -n "Checking: $description... "
    
    matches=$(grep -r "$pattern" --include="$files" "$PROJECT_ROOT" 2>/dev/null | grep -v "docs/internal/" | grep -v ".git/")
    
    if [ -n "$matches" ]; then
        echo -e "${RED}FOUND${NC}"
        echo "$matches" | head -5
        ISSUES=$((ISSUES + 1))
    else
        echo -e "${GREEN}OK${NC}"
    fi
}

# 1. Check for Google/Gemini references in public files
echo "=== 1. Google/Gemini References ==="
check_pattern "Google Gemini" "Google Gemini" "*.html,*.cs,*.md,*.json"
check_pattern "Gemini Nano" "Gemini Nano" "*.html,*.cs,*.md,*.json"
check_pattern "Powered by Gemini" "Powered by Gemini" "*.html,*.cs,*.md,*.json"
check_pattern "Gemini Integration" "Gemini Integration link" "*.html"
echo ""

# 2. Check for incorrect copyright
echo "=== 2. Copyright Check ==="
check_pattern "Tooltip AI × Google" "Incorrect copyright with Google" "*.html,*.md"
check_pattern "©.*Google" "Copyright mentioning Google" "*.html,*.md"
echo ""

# 3. Check for partnership claims
echo "=== 3. Partnership Claims ==="
check_pattern "Alianza:" "Alianza claims" "*.md"
check_pattern "Partner with" "Partnership claims" "*.html,*.cs"
check_pattern "Integrated with" "Integration claims" "*.html,*.cs"
echo ""

# 4. Check for incorrect credits
echo "=== 4. Credits Check ==="
check_pattern "Powered by MiMo" "Incorrect MiMo credit" "*.html,*.md"
check_pattern "Developed by Xiaomi" "Incorrect Xiaomi credit" "*.html,*.md"
echo ""

# 5. Check for external API references
echo "=== 5. External API References ==="
check_pattern "gemini.googleapis.com" "Google API endpoints" "*.cs,*.json"
check_pattern "api.openai.com" "OpenAI API endpoints" "*.cs,*.json"
echo ""

# Summary
echo "========================================"
if [ $ISSUES -eq 0 ]; then
    echo -e "${GREEN}AUDIT PASSED: No issues found${NC}"
else
    echo -e "${RED}AUDIT FAILED: $ISSUES issues found${NC}"
    echo ""
    echo "Review the issues above and fix them before committing."
    echo "See docs/internal/SECURITY-CHECKLIST.md for guidelines."
fi
echo "========================================"
echo ""
