using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class close_to_cathedral_speech_script : MonoBehaviour
{
    private AudioSource speech;
    private bool wasplayed;
    private void Update()
    {
        if (!speech.isPlaying && wasplayed) { 
        Destroy(gameObject);
        }
    }
    void Start()
    {
        wasplayed = false;
        speech = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerTag"))
        {
            wasplayed = true;
            // Play the speech audio
            if (speech != null && !speech.isPlaying)
            {
                speech.Play();
            }
        }
    }
}
