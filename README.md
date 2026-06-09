# Karst

Jogo de mago em **VR** (Meta Quest / OpenXR). O jogador desenha símbolos no ar para
conjurar magias; um modelo CNN reconhece o desenho em tempo real e dispara a magia.

---

## Requisitos

- **Unity 6000.4.10f1** (Unity 6.4). Outras versões não testadas.
- Plataforma **Android** instalada no editor (Quest é Android).
- Headset **Meta Quest** (via Link/Air Link para testar no editor, ou build APK).
- Para o reconhecimento de magias (servidor Python):
  - **Python 3.10**
  - **TensorFlow 2.17.0** → `pip install tensorflow==2.17.0`

---

## Como rodar

### 1. Abrir o projeto
- **Unity Hub:** Add → selecione a pasta do projeto → abra com 6000.4.101.
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

---

## Multiplayer (coop) — Photon Fusion 2

Coop em modo **Shared** (sala fixa, sem servidor dedicado). Cada cliente tem autoridade
sobre o próprio avatar; os outros enxergam via `NetworkTransform`.

### Onde ficam

| Caminho | O que é |
|---|---|
| `Assets/Scripts/Multiplayer/KarstConnectionManager.cs` | Inicia a sessão Fusion (Shared, sala `KarstCoop`, máx. 4) e faz o `Spawn` do avatar quando cada jogador entra |
| `Assets/Scripts/Multiplayer/NetworkAvatarDriver.cs` | No avatar de rede. No dono local, espelha o `PlayerModel` existente (tag `PlayerTag`); no remoto, o `NetworkTransform` move pela rede |
| `Assets/Prefabs/NetworkAvatar.prefab` | Avatar de rede: `NetworkObject` + `NetworkTransform` + `NetworkAvatarDriver` + corpo visível |
| `Assets/Photon/` | SDK Photon Fusion 2 |
| Cena `2 Game Scene` → objeto `Network` | Carrega o `KarstConnectionManager` (campo Player Prefab = `NetworkAvatar`) |

App Id do Photon configurado em `Tools > Fusion > Realtime Settings` (app "Karst" no dashboard).

### Como testar

A rede (conectar + spawn) roda no editor — confira no Console:
`[Karst] Conectado na sala 'KarstCoop'` e `[Karst] Jogador local ... foi spawnado`.

**Atenção:** o teste visual de "um ver o outro se mexendo" **só funciona no Quest**.
No editor Linux não há câmera VR (OpenXR não roda no Linux), então o avatar fica parado,
e o Multiplayer Play Mode (2 instâncias) crasha no Linux. Para validar o coop de verdade:

1. Faça o build Android e instale em 2 headsets Meta Quest (ou 1 Quest + outra instância).
2. Ambos entram na sala `KarstCoop` automaticamente (`Connect On Start`).
3. Cada jogador vê o avatar do outro se mover.
