using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private EnemyCasterController parentAI;

    private void Start()
    {
        parentAI = GetComponentInParent<EnemyCasterController>();
        if (parentAI == null)
        {
            Debug.LogError("Parent EnemyAI script not found!");
        }
    }

    // This function will be called by the animation event
    public void OnCastSpell()
    {
        if (parentAI != null)
        {
            parentAI.CastSpell();
        }
    }
}
