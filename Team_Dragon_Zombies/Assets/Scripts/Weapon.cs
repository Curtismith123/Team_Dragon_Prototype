using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponID;
    public int damage = 10;
    public float fireRate = 0.5f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public Transform firePoint;

    //shotgun variables
    public int pelletsPerShot = 15;
    public float spreadAngle = 15f;

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
                float angleOffset = Random.Range(-spreadAngle / 2, spreadAngle / 2);
                Quaternion rotation = Quaternion.Euler(firePoint.eulerAngles + new Vector3(0, angleOffset, 0));

                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
                Bullet bulletComponent = bullet.GetComponent<Bullet>();

                if (bulletComponent != null)
                {
                    bulletComponent.SetDamage(damage);
                    bulletComponent.SetSpeed(bulletSpeed);
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
