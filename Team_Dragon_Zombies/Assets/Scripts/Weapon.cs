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
    public AudioClip[] outOfAmmo;
    public float outOfAmmoVol;


    [Header("-----Shoot Point Offset-----")]
    public Vector3 shootPointPosition; // Position relative to the weapon model
                                       //^^^ to properly use this place the weapon model in the player models hand at t pose with the barrel pointed away from the player in line with the arm ...
                                       // move the shoot pos to the end of the barrel and not the values in the transform 
                                       // assign these values to the vector 3 offest of the scriptable object then zero the shoot pos transform position and delet weapon model



}