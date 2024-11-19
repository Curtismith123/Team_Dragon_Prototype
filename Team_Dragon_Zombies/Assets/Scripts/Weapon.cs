using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class Weapon : ScriptableObject
{
    [Header("-----Weapon Stats-----")]
    public GameObject weaponModel;
    [Range(1, 50)] public int shootDamage;
    [Range(1, 250)] public int shootDist;
    [Range(0.15f, 10)] public float shootRate;
    [Range(1, 150)] public float bulletSpeed = 20f;
    public int ammoCur, ammoMax;
    [Range(1, 10)] public float bulletDestroyTime = 5f;

    [Header("-----Shotgun Stats-----")]
    [Header("1 for single shot guns, 2+ for shotguns or other")]
    [Range(1, 15)] public int pelletsPerShot = 1;
    [Header("0.01f for single shot guns, 2-5 for shotgun or other")]
    [Range(0.01f, 5)] public float spreadAngle = 0.01f;

    [Header("-----Misc Components-----")]
    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    public float shootVol;

}