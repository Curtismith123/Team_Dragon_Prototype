using UnityEngine;

public class FindShelter : MonoBehaviour
{
    public GameObject FindShelterTesst;
    public AudioSource winAudio;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Player")
        {
            FindShelterTesst.SetActive(true);
            winAudio.Play();
            Time.timeScale = 0f;
        }
    }
}
