using Fusion;
using UnityEngine;

/// <summary>
/// Fase 1 do coop. Vai no prefab "NetworkAvatar" (corpo visivel + NetworkObject + NetworkTransform).
/// NAO mexe no jogador single-player existente.
///
/// - Dono local (HasStateAuthority): o avatar de rede ESPELHA o avatar local que ja existe
///   no jogo (o "PlayerModel", que tem a tag PlayerTag). Assim a posicao do jogador local
///   vai pra rede via NetworkTransform, e os outros enxergam.
/// - Jogador remoto (proxy): nao faz nada; o NetworkTransform move o avatar com os dados da rede.
///
/// Espelhar o PlayerModel (em vez de Camera.main) funciona tanto no editor quanto no Quest,
/// sem depender de uma camera com tag MainCamera (que esta cena nao tem).
/// </summary>
public class NetworkAvatarDriver : NetworkBehaviour
{
    [Tooltip("Tag do avatar local existente que sera espelhado. No projeto e 'PlayerTag'.")]
    [SerializeField] private string localTargetTag = "PlayerTag";

    [Tooltip("Se nao achar pela tag, tenta usar Camera.main (a cabeca do VR).")]
    [SerializeField] private bool useMainCameraFallback = true;

    [Tooltip("Partes do avatar a esconder pro proprio dono (voce esta DENTRO do seu corpo). " +
             "Deixe vazio pra ver o proprio avatar tambem.")]
    [SerializeField] private GameObject[] hideForOwner;

    private Transform localTarget;

    public bool IsLocal => Object != null && Object.HasStateAuthority;

    public override void Spawned()
    {
        if (IsLocal)
        {
            localTarget = ResolveLocalTarget();
            if (localTarget == null)
                Debug.LogWarning($"[Karst] Avatar local nao encontrado (tag '{localTargetTag}' nem Camera.main). " +
                                 "O avatar de rede vai ficar parado no ponto de spawn.");
            else
            {
                // Snap inicial pra nao nascer enterrado no ponto de spawn.
                transform.position = localTarget.position;
                transform.rotation = localTarget.rotation;
            }

            if (hideForOwner != null)
                foreach (var go in hideForOwner)
                    if (go != null) go.SetActive(false);
        }

        gameObject.name = IsLocal ? "NetworkAvatar (Local)" : "NetworkAvatar (Remoto)";
    }

    private Transform ResolveLocalTarget()
    {
        if (!string.IsNullOrEmpty(localTargetTag))
        {
            var go = GameObject.FindWithTag(localTargetTag);
            if (go != null) return go.transform;
        }
        if (useMainCameraFallback && Camera.main != null) return Camera.main.transform;
        return null;
    }

    public override void FixedUpdateNetwork()
    {
        if (IsLocal) Follow();
    }

    public override void Render()
    {
        if (IsLocal) Follow();
    }

    private void Follow()
    {
        if (localTarget == null)
        {
            localTarget = ResolveLocalTarget();
            if (localTarget == null) return;
        }
        transform.position = localTarget.position;
        transform.rotation = localTarget.rotation;
    }
}
