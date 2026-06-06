using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightSphereController : MonoBehaviour
{
    public Transform rightHandTransform;
    private Collider rightSphereCollider;

    private void Start()
    {
        rightSphereCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        rightSphereCollider.transform.forward = rightHandTransform.forward;
        rightSphereCollider.transform.position = rightHandTransform.position;
    }
}
