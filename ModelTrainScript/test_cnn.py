"""
Testa o CNN de reconhecimento de feitiços direto, sem Unity.

Carrega um modelo .h5 e avalia contra as imagens rotuladas em BufferImages/,
usando exatamente o mesmo pre-processamento do jogo (UnityPython.py):
grayscale -> resize 128x128 -> reshape (1,128,128,1) -> /255.

Uso:
    python test_cnn.py [modelo.h5] [--limit N] [--images DIR]

Exemplos:
    python test_cnn.py                                  # usa spell_recognition_model.h5, todas as imagens
    python test_cnn.py ../Assets/Scripts/UnityPython/models/calangos.h5
    python test_cnn.py --limit 30                       # 30 imagens por classe (mais rapido)
"""
import argparse
import os
import sys
import numpy as np
from PIL import Image
import tensorflow as tf

# Ordem das classes = ordem alfabetica das pastas = ordem de treino (igual ao UnityPython.py).
CLASS_NAMES = ['fireball', 'frostbeam', 'heal', 'meteor', 'others', 'shield', 'summon', 'teleport']

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_MODEL = os.path.join(HERE, '..', 'Assets', 'Scripts', 'UnityPython', 'models', 'spell_recognition_model.h5')
DEFAULT_IMAGES = os.path.join(HERE, '..', 'Assets', 'Scripts', 'UnityPython', 'BufferImages')


def preprocess(path):
    """Mesmo pre-processo de preprocess_image() no UnityPython.py."""
    img = Image.open(path).convert('L').resize((128, 128))
    arr = np.array(img).reshape(1, 128, 128, 1).astype('float32') / 255.0
    return arr


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('model', nargs='?', default=DEFAULT_MODEL, help='caminho do .h5')
    ap.add_argument('--images', default=DEFAULT_IMAGES, help='pasta BufferImages')
    ap.add_argument('--limit', type=int, default=0, help='max imagens por classe (0 = todas)')
    args = ap.parse_args()

    print(f"Modelo:  {os.path.normpath(args.model)}")
    print(f"Imagens: {os.path.normpath(args.images)}\n")

    model = tf.keras.models.load_model(args.model)

    n_classes = len(CLASS_NAMES)
    confusion = np.zeros((n_classes, n_classes), dtype=int)  # [real][previsto]
    per_class_total = np.zeros(n_classes, dtype=int)
    per_class_ok = np.zeros(n_classes, dtype=int)

    for ci, cls in enumerate(CLASS_NAMES):
        folder = os.path.join(args.images, cls)
        if not os.path.isdir(folder):
            print(f"  (sem pasta para '{cls}', pulando)")
            continue
        files = [f for f in os.listdir(folder) if f.lower().endswith(('.png', '.jpg', '.jpeg'))]
        files.sort()
        if args.limit:
            files = files[:args.limit]

        for f in files:
            try:
                x = preprocess(os.path.join(folder, f))
            except Exception as e:
                print(f"  erro lendo {f}: {e}")
                continue
            pred = model.predict(x, verbose=0)[0]
            pi = int(np.argmax(pred))
            confusion[ci][pi] += 1
            per_class_total[ci] += 1
            if pi == ci:
                per_class_ok[ci] += 1

        acc = per_class_ok[ci] / per_class_total[ci] if per_class_total[ci] else 0
        print(f"  {cls:<10} {per_class_ok[ci]:>4}/{per_class_total[ci]:<4}  acc={acc:6.1%}")

    total = per_class_total.sum()
    correct = per_class_ok.sum()
    print(f"\nACURACIA GERAL: {correct}/{total} = {(correct/total if total else 0):.1%}\n")

    # Matriz de confusao (linhas = real, colunas = previsto)
    print("Matriz de confusao (linha=real, coluna=previsto):")
    header = "real\\prev  " + " ".join(f"{c[:4]:>5}" for c in CLASS_NAMES)
    print(header)
    for ci, cls in enumerate(CLASS_NAMES):
        row = " ".join(f"{confusion[ci][pj]:>5}" for pj in range(n_classes))
        print(f"{cls:<10} {row}")


if __name__ == '__main__':
    main()
