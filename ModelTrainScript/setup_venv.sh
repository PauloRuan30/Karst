#!/bin/bash
# Cria a venv Python do reconhecimento de feitiços (CNN) se ainda não existir.
# Idempotente: se a venv já tem TensorFlow, sai na hora.
# Chamado automaticamente pelo pythonConnector quando a venv não existe,
# ou pode ser rodado à mão: ./ModelTrainScript/setup_venv.sh
set -e

HERE="$(cd "$(dirname "$0")" && pwd)"
VENV="$HERE/.venv"
PY="$VENV/bin/python"

# Já pronta?
if [ -x "$PY" ] && "$PY" -c "import tensorflow" >/dev/null 2>&1; then
    echo "[setup_venv] venv já pronta (TensorFlow presente)."
    exit 0
fi

# Acha um python 3.10/3.11/3.12
PYBIN=""
for c in python3.10 python3.11 python3.12 python3; do
    command -v "$c" >/dev/null 2>&1 && { PYBIN="$c"; break; }
done
[ -z "$PYBIN" ] && { echo "[setup_venv] ERRO: nenhum python3 encontrado no PATH."; exit 1; }

echo "[setup_venv] criando venv com $PYBIN em $VENV ..."
"$PYBIN" -m venv "$VENV"
"$PY" -m pip install --upgrade pip -q
echo "[setup_venv] instalando tensorflow==2.17.0 + pillow + numpy (download grande, pode demorar) ..."
"$PY" -m pip install "tensorflow==2.17.0" pillow numpy
echo "[setup_venv] pronto. Pode dar Play."
