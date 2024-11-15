using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class Weapon : ScriptableObject
{

    public GameObject weaponModel;
    public int shootDamage;
    public int shootDist;
    public float shootRate;
    public float bulletSpeed = 20f;
    public int ammoCur, ammoMax;
    public float bulletDestroyTime = 5f;

    //shotgun stats
    public int pelletsPerShot = 1;
    public float spreadAngle = 0.01f;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    public float shootVol;


    //add public vec 3, instantiate bullet from enemy ai shoot pos + offset
}
