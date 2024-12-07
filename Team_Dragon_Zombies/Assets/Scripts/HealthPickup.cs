using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] AudioClip[] audHeal;
    [SerializeField][Range(0, 1)] float audHealVol;


    private void OnTriggerEnter(Collider other)
    {
        //player reference for method & audiosource
        PlayerController player = other.GetComponent<PlayerController>();

        if (other.CompareTag("Player"))
        {
            
            player.HealToFull();
            PlayHealSound(player);
            Destroy(gameObject);
        }
    }

    //Plays the audio for pickup
    private void PlayHealSound(PlayerController player)
    {
        if (audHeal.Length > 0 )
        {
            AudioSource playerAudio = player.GetComponent<AudioSource>();
            AudioClip clip = audHeal[Random.Range(0, audHeal.Length)];
            playerAudio.PlayOneShot(clip, audHealVol);
        }
        //debugger
        else { Debug.LogWarning("No heal audio clip"); }
    }

}
