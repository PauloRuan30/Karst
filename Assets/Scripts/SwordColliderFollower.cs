using UnityEngine;

public class SwordColliderFollower : MonoBehaviour
{
    public Transform swordTransform;
    private Collider swordCollider;

    private void Start()
    {
        swordCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        swordCollider.transform.forward = swordTransform.forward;
        swordCollider.transform.position = swordTransform.position;
        Quaternion additionalRotation = Quaternion.Euler(-90, 0, 0);
        swordCollider.transform.rotation *= additionalRotation;
    }
}