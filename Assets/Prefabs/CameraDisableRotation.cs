using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDisableRotation : MonoBehaviour
{
    // Start is called before the first frame update
    private Quaternion my_rotation;
    // Start is called before the first frame update
    void Start()
    {
        my_rotation = this.transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.rotation = my_rotation;

    }
}
