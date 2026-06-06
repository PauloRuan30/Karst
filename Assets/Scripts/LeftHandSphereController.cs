using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHandSphereController : MonoBehaviour
{
    public Transform LeftHandTransform;
    private Collider LeftSphereCollider;

    private void Start()
    {
        LeftSphereCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        LeftSphereCollider.transform.forward = LeftHandTransform.forward;
        LeftSphereCollider.transform.position = LeftHandTransform.position;
    }
}
