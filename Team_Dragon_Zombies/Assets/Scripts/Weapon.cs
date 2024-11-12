using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponID;
    public int damage = 10;
    public float fireRate = 0.5f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public Transform firePoint;

    // shotgun variables
    public int pelletsPerShot = 15;
    public float spreadAngle = 15f;

    public float bulletDestroyTime = 5f; //how long bullet last

    private bool canFire = true;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (canFire)
        {
            for (int i = 0; i < pelletsPerShot; i++)
            {
                float horizontalAngle = Random.Range(-spreadAngle / 2, spreadAngle / 2);
                float verticalAngle = Random.Range(-spreadAngle / 2, spreadAngle / 2);

                Quaternion rotation = Quaternion.Euler(firePoint.eulerAngles + new Vector3(verticalAngle, horizontalAngle, 0));

                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
                Bullet bulletComponent = bullet.GetComponent<Bullet>();

                if (bulletComponent != null)
                {
                    bulletComponent.SetDamage(damage);
                    bulletComponent.SetSpeed(bulletSpeed);
                    bulletComponent.SetDestroyTime(bulletDestroyTime);
                }
            }

            StartCoroutine(FireCooldown());
        }
    }

    IEnumerator FireCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }
}
