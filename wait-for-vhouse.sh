#!/bin/bash
# Script que espera hasta que VHouse esté respondiendo en localhost:5000

MAX_WAIT=120  # Máximo 120 segundos (2 minutos)
WAIT_INTERVAL=2  # Verificar cada 2 segundos
URL="http://localhost:5000"

echo "🕐 Esperando a que VHouse esté disponible en $URL..."
echo "   (Máximo ${MAX_WAIT}s)"

elapsed=0
while [ $elapsed -lt $MAX_WAIT ]; do
    # Intentar conectar con curl
    if curl -s -o /dev/null -w "%{http_code}" "$URL" | grep -q "200\|302\|404"; then
        echo "✅ VHouse está respondiendo!"
        echo "   Abre $URL en tu navegador"
        exit 0
    fi

    echo -n "."
    sleep $WAIT_INTERVAL
    elapsed=$((elapsed + WAIT_INTERVAL))
done

echo ""
echo "❌ Timeout: VHouse no respondió después de ${MAX_WAIT}s"
echo "   Verifica los logs para ver posibles errores"
exit 1