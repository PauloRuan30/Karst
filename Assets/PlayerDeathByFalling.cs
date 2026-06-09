using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerDeathByFalling : MonoBehaviour
{
    [SerializeField] GameObject DeathScreenCanvas;
    [SerializeField] GameObject rayInteractor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "PlayerTag") {
            Player.instance.PlayerDie();
        }
    }

}
