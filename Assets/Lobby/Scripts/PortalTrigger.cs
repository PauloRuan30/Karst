using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [Tooltip("Se marcado, ignora a tag e aceita qualquer collider (apenas para debug).")]
    [SerializeField] private bool acceptAnyCollider = false;

    [Tooltip("Tag que o objeto deve ter para acionar a transicao.")]
    [SerializeField] private string requiredTag = "PlayerTag";

    private void Reset()
    {
        // Garante que tem um collider configurado como trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PortalTrigger] OnTriggerEnter disparado por: '{other.name}' (tag='{other.tag}', root='{other.transform.root.name}')", this);

        bool tagOk = acceptAnyCollider || other.CompareTag(requiredTag) || other.transform.root.CompareTag(requiredTag);
        if (!tagOk)
        {
            Debug.Log($"[PortalTrigger] Ignorado: tag '{other.tag}' nao corresponde a '{requiredTag}'.", this);
            return;
        }

        var lobby = FindObjectOfType<LobbyController>();
        if (lobby == null)
        {
            Debug.LogError("[PortalTrigger] LobbyController nao encontrado na cena! Crie um GameObject com o componente LobbyController.", this);
            return;
        }

        Debug.Log("[PortalTrigger] Chamando LobbyController.EnterGame()", this);
        lobby.EnterGame();
    }
}
