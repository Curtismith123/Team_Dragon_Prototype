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
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();

            if (bulletComponent != null)
            {
                bulletComponent.SetDamage(damage);
                bulletComponent.SetSpeed(bulletSpeed);
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
