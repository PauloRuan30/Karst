using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnterPortalController : MonoBehaviour
{
    [SerializeField] float lifeTime = 5f;
    // Start is called before the first frame update
    GameObject ExitPortal;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    private void OnTriggerEnter(Collider other)
    {
        ExitPortal = GameObject.Find("PortalExit");
        GameObject obj = other.gameObject;
        if(obj.layer!=LayerMask.NameToLayer("Teleport"))
        obj.transform.position = ExitPortal.transform.position;
    }

}
