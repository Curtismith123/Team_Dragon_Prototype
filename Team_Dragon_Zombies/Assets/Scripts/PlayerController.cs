using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    [Header("-----Class-----")]
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] CharacterController controller;
    [Header("-----Player Stats-----")]
    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    [Header("-----Weapon Info-----")]
    [SerializeField] List<Weapon> weaponList = new List<Weapon>();
    [SerializeField] GameObject weaponModel;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;
    float shootDamage;
    float shootDist;
    float shootRate;
    float bulletSpeed;
    int pelletsPerShot;
    float spreadAngle;

    Vector3 moveDir;
    Vector3 playerVel;
    bool isSprinting;
    bool isShooting;
    int jumpCount;
    int HPOrig;
    int selectedWeapon;
    private bool canFire = true;

    void Start()
    {
        HPOrig = HP;
        updatePlayerUI();
    }

    void Update()
    {
        if (!gameManager.instance.IsPaused)
        {
            movement();
            sprint();
        }

        if (weaponList.Count > 0)
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * weaponList[selectedWeapon].shootDist, Color.red);
            selectWeapon();
            reload();
        }

        if (Input.GetButton("Fire1") && weaponList.Count > 0 && weaponList[selectedWeapon].ammoCur > 0 && canFire)
        {
            StartCoroutine(shoot());
            StartCoroutine(FireCooldown());
        }
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        moveDir = (transform.forward * Input.GetAxis("Vertical")) +
          (transform.right * Input.GetAxis("Horizontal"));

        controller.Move(moveDir * speed * Time.deltaTime);

        jump();

        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }



    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }


    IEnumerator shoot()
    {
        isShooting = true;
        Weapon currentWeapon = weaponList[selectedWeapon];

        if (currentWeapon.ammoCur <= 0)
        {
            // sound here for click if out of ammo
            yield break;
        }

        Debug.Log("Ammo before shot: " + currentWeapon.ammoCur);

        gameManager.instance.ammoUpdate(currentWeapon.ammoCur);
        currentWeapon.ammoCur--;

        Debug.Log("Ammo after shot: " + currentWeapon.ammoCur);

        StartCoroutine(flashMuzzle());

        int bulletsToFire = currentWeapon.pelletsPerShot > 1 ? currentWeapon.pelletsPerShot : 1;

        for (int i = 0; i < bulletsToFire; i++)
        {
            GameObject newBullet = Instantiate(bullet, shootPos.position, Quaternion.identity);

            Vector3 shootDirection = shootPos.forward;

            if (bulletsToFire > 1)
            {
                float horizontalAngle = Random.Range(-spreadAngle / 2, spreadAngle / 2);
                float verticalAngle = Random.Range(-spreadAngle / 2, spreadAngle / 2);
                float depthAngle = Random.Range(-spreadAngle / 2, spreadAngle / 2);

                Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, depthAngle);
                shootDirection = rotation * shootDirection;
            }

            newBullet.transform.forward = shootDirection;

            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            bulletScript.SetSpeed(currentWeapon.bulletSpeed);
            bulletScript.SetDamage(currentWeapon.shootDamage);
            bulletScript.SetDestroyTime(currentWeapon.bulletDestroyTime);
        }

        yield return new WaitForSeconds(currentWeapon.shootRate);
        isShooting = false;
    }

    IEnumerator flashMuzzle()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(flashDmage());

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }

    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    IEnumerator flashDmage()
    {
        gameManager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageScreen.SetActive(false);

    }

    public void getWeaponStats(Weapon weapon)
    {
        if (!weaponList.Contains(weapon))
        {
            weaponList.Add(weapon);
        }
        selectedWeapon = weaponList.Count - 1;
        shootDamage = weaponList[selectedWeapon].shootDamage;
        shootDist = weaponList[selectedWeapon].shootDist;
        shootRate = weaponList[selectedWeapon].shootRate;
        bulletSpeed = weaponList[selectedWeapon].bulletSpeed;
        pelletsPerShot = weaponList[selectedWeapon].pelletsPerShot;
        spreadAngle = weaponList[selectedWeapon].spreadAngle;

        foreach (Transform child in weaponModel.transform)
        {
            Destroy(child.gameObject);
        }
        ApplyWeaponParts(weapon.weaponModel.transform);
    }

    void ApplyWeaponParts(Transform weaponPart)
    {
        foreach (Transform child in weaponPart)
        {
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();

            if (meshFilter != null && meshRenderer != null)
            {
                GameObject weaponPartObj = new GameObject(child.name);
                weaponPartObj.transform.SetParent(weaponModel.transform, false);
                weaponPartObj.transform.localPosition = child.localPosition;
                weaponPartObj.transform.localRotation = child.localRotation;
                weaponPartObj.transform.localScale = child.localScale;

                MeshFilter partMeshFilter = weaponPartObj.AddComponent<MeshFilter>();
                MeshRenderer partMeshRenderer = weaponPartObj.AddComponent<MeshRenderer>();
                partMeshFilter.sharedMesh = meshFilter.sharedMesh;
                partMeshRenderer.sharedMaterial = meshRenderer.sharedMaterial;
            }
            ApplyWeaponParts(child);
        }
    }

    void selectWeapon()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedWeapon < weaponList.Count - 1)
        {
            selectedWeapon++;
            changeWeapon();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedWeapon > 0)
        {
            selectedWeapon--;
            changeWeapon();
        }
        selectedWeapon = Mathf.Clamp(selectedWeapon, 0, weaponList.Count - 1);
    }

    void changeWeapon()
    {

        foreach (Transform child in weaponModel.transform)
        {
            Destroy(child.gameObject);
        }

        ApplyWeaponParts(weaponList[selectedWeapon].weaponModel.transform);

        MeshFilter newMeshFilter = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>();
        MeshRenderer newMeshRenderer = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>();

        if (newMeshFilter != null)
        {
            weaponModel.GetComponent<MeshFilter>().sharedMesh = newMeshFilter.sharedMesh;
        }
        else
        {
            Debug.LogWarning($"Weapon {weaponList[selectedWeapon].name} is missing a MeshFilter component.");
        }

        if (newMeshRenderer != null)
        {
            weaponModel.GetComponent<MeshRenderer>().sharedMaterial = newMeshRenderer.sharedMaterial;
        }
        else
        {
            Debug.LogWarning($"Weapon {weaponList[selectedWeapon].name} is missing a MeshRenderer component.");
        }

        shootDamage = weaponList[selectedWeapon].shootDamage;
        shootDist = weaponList[selectedWeapon].shootDist;
        shootRate = weaponList[selectedWeapon].shootRate;
        bulletSpeed = weaponList[selectedWeapon].bulletSpeed;
        pelletsPerShot = weaponList[selectedWeapon].pelletsPerShot;
        spreadAngle = weaponList[selectedWeapon].spreadAngle;

    }

    IEnumerator FireCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(weaponList[selectedWeapon].shootRate);
        canFire = true;
    }

    void reload()
    {
        if (Input.GetButtonDown("Reload") && weaponList.Count > 0)
        {
            Weapon currentWeapon = weaponList[selectedWeapon];
            currentWeapon.ammoCur = Mathf.Min(currentWeapon.ammoMax, currentWeapon.ammoCur + currentWeapon.ammoMax); // Ensure ammo doesn't exceed max
        }
    }

}