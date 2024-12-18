using UnityEngine;

public class Equip : MonoBehaviour
{
    private enum PickupType { Weapon, HP, Stamina, Ammo, Key, ConversionCharge }

    [Header("Pickup Settings")]
    [SerializeField] private PickupType type;
    [SerializeField] private Weapon weapon;
    [SerializeField] private string keyID;
    [SerializeField] private float conversionChargeAmount = 20f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip weaponPickupClip;
    [SerializeField] private AudioClip keyPickupClip;

    private AudioSource playerAudioSource;

    void Start()
    {
        if (type == PickupType.Weapon && weapon != null)
        {
            weapon.ammoCur = weapon.ammoMax;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerAudioSource = other.GetComponent<AudioSource>();

            if (playerAudioSource == null)
            {
                Debug.LogWarning("Player does not have an AudioSource component.");
            }

            switch (type)
            {
                case PickupType.Weapon:
                    if (weapon != null)
                    {
                        gameManager.instance.playerScript.getWeaponStats(weapon);
                        gameManager.instance.ammoUpdate(weapon.ammoCur);
                        PlaySound(weaponPickupClip);
                    }
                    else
                    {
                        Debug.LogWarning("Weapon reference is missing on pickup object.");
                    }
                    break;

                case PickupType.Key:
                    gameManager.instance.AddKey(keyID);
                    PlaySound(keyPickupClip);
                    break;

                case PickupType.ConversionCharge:
                    gameManager.instance.playerScript.AddConversionGauge(conversionChargeAmount);
                    break;

                default:
                    break;
            }

            Destroy(gameObject);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (playerAudioSource != null && clip != null)
        {
            playerAudioSource.PlayOneShot(clip);
        }
        else
        {
            if (playerAudioSource == null)
                Debug.LogWarning("Player AudioSource is missing. Cannot play pickup sound.");
            if (clip == null)
                Debug.LogWarning("AudioClip is not assigned for this pickup type.");
        }
    }
}
