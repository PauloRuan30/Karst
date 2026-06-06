using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealController : MonoBehaviour
{
    [SerializeField] float liveTime = 3f;
    [SerializeField] float regenerationAmount = 10f;
    GameObject playerModel;
    Player playerScript;
    // Start is called before the first frame update
    void Start()
    {
        playerModel = GameObject.Find("PlayerModel");
        playerScript = playerModel.GetComponent<Player>();
        playerScript.PlayerUpdateRegenerationSpeed((int)regenerationAmount);
        Destroy(gameObject,liveTime);
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 2 * player.transform.localScale.y, player.transform.position.z);
        gameObject.transform.position = playerModel.transform.position - 2*Vector3.up;
        gameObject.transform.rotation = playerModel.transform.rotation;
    }
    private void OnDestroy()
    {
        playerScript.PlayerUpdateRegenerationSpeed(0);

    }
}
