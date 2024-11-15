using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equip : MonoBehaviour
{
    enum pickupType { weapon, HP, stamina }
    [SerializeField] pickupType type;
    [SerializeField] Weapon weapon;


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
            gameManager.instance.playerScript.getWeaponStats(weapon);
            Destroy(gameObject);
        }
    }


}
