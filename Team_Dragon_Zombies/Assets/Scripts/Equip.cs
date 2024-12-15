using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip : MonoBehaviour
{
    enum pickupType { weapon, HP, stamina, ammo, key, conversionCharge }
    [SerializeField] pickupType type;
    [SerializeField] Weapon weapon;
    [SerializeField] string keyID;
    [SerializeField] float conversionChargeAmount = 20f;
    private AudioSource audioSource;
    public AudioClip clip;

    void Start()
    {
        if (type == pickupType.weapon)
        {
            weapon.ammoCur = weapon.ammoMax;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (type)
            {
                case pickupType.weapon:
                    gameManager.instance.playerScript.getWeaponStats(weapon);
                    gameManager.instance.ammoUpdate(weapon.ammoCur);
                    break;

                case pickupType.key:
                    gameManager.instance.AddKey(keyID);
                    break;

                case pickupType.conversionCharge:
                    gameManager.instance.playerScript.AddConversionGauge(conversionChargeAmount);
                    break;
            }

            Destroy(gameObject);
        }
    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}