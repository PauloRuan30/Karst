# Karst

Jogo de mago em **VR** (Meta Quest / OpenXR). O jogador desenha símbolos no ar para
conjurar magias; um modelo CNN reconhece o desenho em tempo real e dispara a magia.

---

## Requisitos

- **Unity 6000.4.4f1** (Unity 6.4). Outras versões não testadas.
- Plataforma **Android** instalada no editor (Quest é Android).
- Headset **Meta Quest** (via Link/Air Link para testar no editor, ou build APK).
- Para o reconhecimento de magias (servidor Python):
  - **Python 3.10**
  - **TensorFlow 2.17.0** → `pip install tensorflow==2.17.0`

---

## Como rodar

### 1. Abrir o projeto
- **Unity Hub:** Add → selecione a pasta do projeto → abra com 6000.4.4f1.
- **Linux (terminal):** `./launch.sh`
  - Acha o editor automaticamente. Para apontar manualmente:
    `UNITY_PATH=/caminho/Editor/Unity ./launch.sh`
    ou `UNITY_BASE=/pasta/com/as/versoes ./launch.sh`

### 2. Cena inicial
Abra **`Assets/Scenes/1 Start Scene.unity`** e dê **Play**.
Fluxo do jogo: `1 Start Scene` (menu/perfil) → `2 Game Scene` (gameplay).

### 3. Reconhecimento de magias (Python)
O Unity escuta TCP na porta **25001**; o script Python conecta e devolve a previsão.

1. Dê **Play** no Unity primeiro (ele abre o listener).
2. Rode o servidor:
   ```bash
   python "Assets/Scripts/UnityPython/UnityPython.py"
   ```
Magias reconhecidas: `fireball`, `frostbeam`, `heal`, `meteor`, `shield`, `summon`,
`teleport` (+ `others`).

### 4. Rodar no Quest
- **Link/Air Link:** conecte o headset e dê Play — renderiza no HMD via OpenXR.
- **Standalone:** `File > Build Settings > Android > Build` gera o APK.

---

## Onde estão as coisas

| Caminho | O que é |
|---|---|
| `Assets/Scenes/` | Cenas do jogo (`1 Start Scene`, `2 Game Scene`) |
| `Assets/Prefabs/Player.prefab` | Jogador (rig VR, cajado, PlayerModel) |
| `Assets/Scripts/` | Gameplay: `CastSystem`, `Draw`, `Player`, `EnemySpawner`, `Projectile`, mãos, inimigos |
| `Assets/Scripts/SceneScripts/` | Menus, transição de cena, áudio (`GameStartMenu`, `SceneTransitionManager`) |
| `Assets/Scripts/UnityPython/` | Servidor Python de reconhecimento + modelos `.h5` em `models/` |
| `ModelTrainScript/trainmodel.py` | Treino do modelo CNN |
| `Assets/XR/`, `Assets/XRI/` | Configuração XR / OpenXR / Interaction Toolkit |
| `launch.sh` / `launch.bat` | Atalhos para abrir o projeto no editor correto |
| `DOCUMENTATION/` | Documentação e apresentações do projeto |
