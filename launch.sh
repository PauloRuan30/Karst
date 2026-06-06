#!/bin/bash

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
UNITY_VERSION=$(grep "m_EditorVersion:" "$PROJECT_DIR/ProjectSettings/ProjectVersion.txt" | awk '{print $2}')

# Override total: aponte direto pro binário do Unity, ex:
#   UNITY_PATH=/caminho/Editor/Unity ./launch.sh
# Ou aponte só a pasta-base onde ficam as versões (cada uma em <base>/<versao>/Editor/Unity):
#   UNITY_BASE=/media/fernando-bezerra/Files1/APPS/Unity/Editors ./launch.sh
if [ -n "$UNITY_PATH" ] && [ -f "$UNITY_PATH" ]; then
    : # usa o caminho passado direto
else
    # Bases candidatas, em ordem de prioridade. $UNITY_BASE entra primeiro se definido.
    UNITY_BASES=(
        "$UNITY_BASE"
        "/media/fernando-bezerra/Files1/APPS/Unity/Editors"
        "$HOME/Unity/Hub/Editor"
        "/opt/Unity/Hub/Editor"
        "/opt/unity/Editor"
    )
    UNITY_PATH=""
    for base in "${UNITY_BASES[@]}"; do
        [ -z "$base" ] && continue
        candidate="$base/$UNITY_VERSION/Editor/Unity"
        if [ -f "$candidate" ]; then
            UNITY_PATH="$candidate"
            break
        fi
    done
fi

if [ -z "$UNITY_PATH" ] || [ ! -f "$UNITY_PATH" ]; then
    echo "Unity $UNITY_VERSION não encontrado."
    echo "Procurei em:"
    for base in "${UNITY_BASES[@]}"; do
        [ -z "$base" ] && continue
        echo "  - $base/$UNITY_VERSION/Editor/Unity"
    done
    echo "Defina o caminho manualmente:"
    echo "  UNITY_PATH=/caminho/para/Editor/Unity ./launch.sh"
    echo "  ou UNITY_BASE=/pasta/com/as/versoes ./launch.sh"
    exit 1
fi

UNITY_ARGS="-projectPath $PROJECT_DIR"
ENV_PREFIX=""

GPU=$(lspci 2>/dev/null | grep -iE "vga|3d|display" | head -1)

if echo "$GPU" | grep -qi "intel"; then
    ENV_PREFIX="LIBGL_DRI3_DISABLE=1"
    UNITY_ARGS="$UNITY_ARGS -force-glcore"
    echo "Intel GPU detectado — DRI3 desativado, forçando OpenGL Core"
elif echo "$GPU" | grep -qi "nvidia"; then
    echo "NVIDIA GPU detectado — configuração padrão"
elif echo "$GPU" | grep -qi "amd\|radeon\|ati"; then
    echo "AMD GPU detectado — configuração padrão"
else
    echo "GPU não identificada — usando configuração padrão"
fi

echo "Iniciando Unity $UNITY_VERSION..."
eval "env $ENV_PREFIX \"$UNITY_PATH\" $UNITY_ARGS"
