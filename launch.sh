#!/bin/bash

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
UNITY_VERSION=$(grep "m_EditorVersion:" "$PROJECT_DIR/ProjectSettings/ProjectVersion.txt" | awk '{print $2}')
UNITY_PATH="$HOME/Unity/Hub/Editor/$UNITY_VERSION/Editor/Unity"

if [ ! -f "$UNITY_PATH" ]; then
    echo "Unity $UNITY_VERSION não encontrado em $UNITY_PATH"
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
