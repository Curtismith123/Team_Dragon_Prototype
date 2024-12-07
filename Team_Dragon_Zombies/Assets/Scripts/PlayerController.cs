using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem.Editor;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour, IDamage
{
    [Header("-----Class-----")]
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] CharacterController controller;

    [Header("-----Player Stats-----")]
    [SerializeField] int HP;

    [Header("-----Sprint Modifiers-----")]
    [SerializeField][Range(1, 10)] int Speed = 5;
    [SerializeField] float maxStamina = 100f;
    [SerializeField][Range(1, 100)] float staminaRegenRate = 10;
    [SerializeField][Range(1, 100)] float staminaDepletionRate = 20f;
    [SerializeField][Range(1, 5)] int sprintMod = 2;
    private float currentStamina;
    private bool isSprinting;

    [Header("-----Jump Modifiers-----")]
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    [Header("-----Animation-----")]
    [SerializeField] Animator anim;
    private bool isAiming;


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
    bool isTwoHanded;
    [SerializeField] GameObject ohPos;
    [SerializeField] GameObject thIdlePos;
    [SerializeField] GameObject thAimPos;
    [SerializeField] float switchTime;

    [Header("-----Effect Selection-----")]
    [SerializeField] private Image currentEffectIcon;
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite lightningIcon;


    private EffectType currentEffect = EffectType.Fire;

    [Header("-----Effect Scritable Objects-----")]
    public StatusEffectSO fireEffect;
    public StatusEffectSO iceEffect;
    public StatusEffectSO lightningEffect;



    [Header("-----Conversion-----")]
    [Header("-----Conversion Gauge-----")]
    [SerializeField] private float maxConversionGauge = 100f;
    [SerializeField] private float conversionGauge = 100f;
    [SerializeField] private float conversionGaugeRefillRate = 5f;
    [SerializeField] private float conversionGaugePerAttempt = 20f;
    [Header("-----Conversion Projectile-----")]
    [SerializeField] GameObject conversionProjectilePrefab;
    [SerializeField] Transform conversionShootPos;
    [SerializeField] float conversionProjectileSpeed = 10f;

    [Header("-----Audio-----")]
    [SerializeField] AudioSource aud;

    [SerializeField] AudioClip[] audJump;
    [SerializeField][Range(0, 1)] float audJumpVol;
    [SerializeField] AudioClip[] audLanding;
    [SerializeField][Range(0, 1)] float audLandingVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField][Range(0, 1)] float audHurtVol;
    [SerializeField] AudioClip[] audSteps;
    [SerializeField] float stepRate = 0.4f;
    [SerializeField][Range(0, 1)] float audStepsVol;

    bool isPlayingSteps;
    [Header("-----Misc-----")]
    public GameObject hat;

    [Header("-----Damage Screen-----")]
    public float intensity;
    public Volume dmgEffect;
    public Volume lowHPEffect;
    PostProcessVolume mDmgEffectVol;
    Vignette mDmgVignette;
    //PostProcessVolume mLowHPEffect;
    //Vignette mLowHPVignette;
    private float intensityOG;

    Vector3 moveDir;
    Vector3 playerVel;
    bool isShooting;
    int jumpCount;
    int HPOrig;
    int SpeedAlt;
    int selectedWeapon;
    private bool canFire = true;
    private bool hasJumped;



    void Start()
    {
        HPOrig = HP;
        SpeedAlt = Speed;
        currentStamina = maxStamina;
        updatePlayerUI();
        hat.SetActive(false);
        currentEffect = EffectType.Fire;
        intensityOG = intensity;




        mDmgEffectVol = dmgEffect.GetComponent<PostProcessVolume>();
        mDmgEffectVol.profile.TryGetSettings<Vignette>(out mDmgVignette);
        mDmgVignette.enabled.Override(false);


        //mLowHPEffect = lowHPEffect.GetComponent<PostProcessVolume>();
        //mLowHPEffect.profile.TryGetSettings<Vignette>(out mLowHPVignette);
        //mLowHPVignette.enabled.Override(false);

    }

    void Update()
    {
        if (!gameManager.instance.IsPaused)
        {
            movement();
            sprint();
        }
        // Update the animator's Speed parameter
        UpdateAnimator();

        UpdateEffectUI();
        if (weaponList.Count > 0)
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * weaponList[selectedWeapon].shootDist, Color.red);
            selectWeapon();
            reload();
        }
        //&& weaponList[selectedWeapon].ammoCur > 0
        if (Input.GetButton("Fire1") && weaponList.Count > 0 && canFire && isAiming)
        {
            ////// Align the weapon model to face forward relative to the player's forward
            //weaponModel.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

            StartCoroutine(shoot());
            StartCoroutine(FireCooldown());
        }

        if (conversionGauge < maxConversionGauge)
        {
            conversionGauge += conversionGaugeRefillRate * Time.deltaTime;
            if (conversionGauge > maxConversionGauge)
            {
                conversionGauge = maxConversionGauge;
            }
            UpdateConversionGaugeUI();
        }
        if (weaponModel != null)
        {
            StartCoroutine(HandleWeaponMovement());
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryConvertEnemy();
        }
        // check bool flip bool listener
        if (controller.isGrounded && anim.GetBool("isGrounded") == false)
        {
            anim.SetBool("isGrounded", true);
        }

        // Infusion Selection
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateEffect();
        }
        // Aim to shoot 
        if (Input.GetButtonDown("Aim"))
        {
            isAiming = true;

        }
        if (Input.GetButtonUp("Aim") && isAiming)
        {
            isAiming = false;

        }

        // Low Health Effect
        if (HP < 20)
        {
            mDmgVignette.intensity.Override(intensityOG);
            mDmgVignette.enabled.Override(true);
            mDmgVignette.intensity.Override(intensityOG + 0.2f);
        }
        if (HP < 10)
        {
            mDmgVignette.enabled.Override(true);
            mDmgVignette.intensity.Override(intensityOG + 0.3f);
        }
    }

    private void RotateEffect()
    {
        // Cycle to the next effect in the enum
        currentEffect = (EffectType)(((int)currentEffect + 1) % System.Enum.GetValues(typeof(EffectType)).Length);

        // Update the UI to display the current effect's icon
        UpdateEffectUI();
    }
    private void UpdateEffectUI()
    {
        switch (currentEffect)
        {
            case EffectType.Fire:
                currentEffectIcon.sprite = fireIcon;
                break;
            case EffectType.Ice:
                currentEffectIcon.sprite = iceIcon;
                break;
            case EffectType.Lightning:
                currentEffectIcon.sprite = lightningIcon;
                break;
        }
    }


    private void UpdateAnimator()
    {
        anim.SetBool("isShooting", isShooting);
        anim.SetBool("isAiming", isAiming);
        anim.SetBool("Hands", isTwoHanded);
        // Set the Speed parameter in the Animator based on movement magnitude
        float movementSpeed = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;
        anim.SetFloat("Speed", movementSpeed * SpeedAlt);
    }

    void movement()
    {
        if (!isPlayingSteps && moveDir.magnitude > 0.3f && controller.isGrounded) { StartCoroutine(playStep()); }
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        moveDir = (transform.forward * Input.GetAxis("Vertical")) +
          (transform.right * Input.GetAxis("Horizontal"));

        controller.Move(moveDir * SpeedAlt * Time.deltaTime);

        jump();

        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

        CheckLanding();

        anim.SetBool("isGrounded", (controller.isGrounded));
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            // trigger jump animation
            if (!hasJumped)
            {
                anim.SetTrigger("jumpTrig");
                anim.SetBool("isGrounded", false);
            }
            else
            {
                anim.SetTrigger("dJumpTrig");
            }

            jumpCount++;
            playerVel.y = jumpSpeed;
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
            hasJumped = true;
        }


    }

    //play landing sound
    void CheckLanding()
    {
        if (controller.isGrounded && hasJumped)
        {
            aud.PlayOneShot(audLanding[Random.Range(0, audLanding.Length)], audLandingVol);
            hasJumped = false;
        }

    }


    //-------------------------------------------Stamina Logic (COMPLETE)
    void sprint()
    {
        if (Input.GetButton("Sprint") && currentStamina > 0)
        {
            if (!isSprinting)
            {
                SpeedAlt *= sprintMod;
                isSprinting = true;
            }

            currentStamina -= staminaDepletionRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
        }
        else
        {
            if (isSprinting)
            {
                SpeedAlt = Speed;
                isSprinting = false;
            }

            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }

        if (currentStamina <= 0 && isSprinting)
        {
            SpeedAlt = Speed;
            isSprinting = false;
        }

        updateStaminaUI();
    }
    //-------------------------------------------

    IEnumerator playStep()
    {
        isPlayingSteps = true;

        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)], audStepsVol);

        if (!isSprinting)
            yield return new WaitForSeconds(stepRate);
        else
            yield return new WaitForSeconds(stepRate / sprintMod);

        isPlayingSteps = false;
    }


    IEnumerator shoot()
    {

        isShooting = true;
        Weapon currentWeapon = weaponList[selectedWeapon];

        if (currentWeapon.ammoCur <= 0)
        {
            //sound here for click if out of ammo
            if (Input.GetButton("Fire1"))
            {

                aud.PlayOneShot(weaponList[selectedWeapon].outOfAmmo[Random.Range(0, weaponList[selectedWeapon].outOfAmmo.Length)], weaponList[selectedWeapon].outOfAmmoVol);
            }

            yield return new WaitForSeconds(0.5f);
            isShooting = false;
            yield break;

        }

        //------------------------Ammo decrament logic (COMPLETE)
        weaponList[selectedWeapon].ammoCur--;
        //Audio for shooting
        aud.PlayOneShot(weaponList[selectedWeapon].shootSound[Random.Range(0, weaponList[selectedWeapon].shootSound.Length)], weaponList[selectedWeapon].shootVol);

        if (weaponList[selectedWeapon].ammoCur <= 0)
        {
            gameManager.instance.ammoUpdate(0);
        }
        else
        {
            gameManager.instance.ammoUpdate(weaponList[selectedWeapon].ammoCur);
        }
        //------------------------

        StartCoroutine(flashMuzzle());

        int bulletsToFire = currentWeapon.pelletsPerShot > 1 ? currentWeapon.pelletsPerShot : 1;

        for (int i = 0; i < bulletsToFire; i++)
        {
            GameObject newBullet = Instantiate(bullet, shootPos.position, Quaternion.identity);

            Vector3 shootDirection = (Camera.main.transform.forward).normalized;

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
            bulletScript.SetAttacker(gameObject);

            switch (currentEffect)
            {
                case EffectType.Fire:

                    bulletScript.statusEffect = fireEffect;
                    break;
                case EffectType.Ice:

                    bulletScript.statusEffect = iceEffect;
                    break;
                case EffectType.Lightning:

                    bulletScript.statusEffect = lightningEffect;
                    break;
            }
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

    public void takeDamage(int amount, GameObject attacker, EffectType? effectType = null)
    {
        HP -= amount;
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
        updatePlayerUI();
        //StartCoroutine(flashDmage());

        StartCoroutine(damageEffect());

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }

    private IEnumerator damageEffect()
    {
        mDmgVignette.enabled.Override(true);
        mDmgVignette.intensity.Override(intensityOG);

        yield return new WaitForSeconds(0.5f);

        while (intensity > 0)
        {
            intensity -= .1f;
            if (intensity < 0)
            {
                intensity = 0;
            }
            mDmgVignette.intensity.Override(intensity);
            yield return new WaitForSeconds(0.1f);
        }
        if (HP <= 20)
        {
            //mDmgVignette.enabled.Override(true);
        }
        else
        {
            mDmgVignette.enabled.Override(false);
        }
    }

    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    //-------------------------------------------Stamina Logic (COMPLETE)
    public void updateStaminaUI()
    {
        gameManager.instance.playerStaminaBar.fillAmount = currentStamina / maxStamina;
    }
    //-------------------------------------------

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
        isTwoHanded = weaponList[selectedWeapon].isTwoHanded;



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

        shootDamage = weaponList[selectedWeapon].shootDamage;
        shootDist = weaponList[selectedWeapon].shootDist;
        shootRate = weaponList[selectedWeapon].shootRate;
        bulletSpeed = weaponList[selectedWeapon].bulletSpeed;
        pelletsPerShot = weaponList[selectedWeapon].pelletsPerShot;
        spreadAngle = weaponList[selectedWeapon].spreadAngle;
        isTwoHanded = weaponList[selectedWeapon].isTwoHanded;


        gameManager.instance.ammoUpdate(weaponList[selectedWeapon].ammoCur);
    }

    IEnumerator FireCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(weaponList[selectedWeapon].shootRate);
        canFire = true;
    }



    //-------------------------------------------AMMO LOGIC (COMPLETE)
    void reload()
    {
        if (Input.GetButtonDown("Reload") && weaponList.Count > 0)
        {
            Weapon currentWeapon = weaponList[selectedWeapon];
            currentWeapon.ammoCur = Mathf.Min(currentWeapon.ammoMax, currentWeapon.ammoCur + currentWeapon.ammoMax);
            gameManager.instance.ammoUpdate(weaponList[selectedWeapon].ammoCur);
        }
    }
    //-------------------------------------------

    public void UpdateConversionGaugeUI()
    {
        gameManager.instance.playerConversionBar.fillAmount = conversionGauge / maxConversionGauge;
    }

    public void AddConversionGauge(float amount)
    {
        conversionGauge += amount;
        if (conversionGauge > maxConversionGauge)
        {
            conversionGauge = maxConversionGauge;
        }
        UpdateConversionGaugeUI();
    }

    void TryConvertEnemy()
    {
        if (conversionGauge >= conversionGaugePerAttempt)
        {
            conversionGauge -= conversionGaugePerAttempt;
            UpdateConversionGaugeUI();

            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f);
            }

            Vector3 shootDirection = (targetPoint - conversionShootPos.position).normalized;

            GameObject newProjectile = Instantiate(conversionProjectilePrefab, conversionShootPos.position, Quaternion.identity);

            newProjectile.transform.forward = shootDirection;

            ConversionProjectile projectileScript = newProjectile.GetComponent<ConversionProjectile>();


            if (projectileScript != null)
            {
                projectileScript.SetSpeed(conversionProjectileSpeed);
                projectileScript.SetDestroyTime(5f);
            }
            else
            {
                Debug.LogWarning("ConversionProjectile script not found on the instantiated projectile.");
            }
        }
        else
        {
            Debug.Log("Not enough conversion gauge to attempt conversion.");
        }
    }

    public void HealToFull()
    {
        HP = HPOrig;
        updatePlayerUI();
    }


    private IEnumerator HandleWeaponMovement()
    {
        while (true)
        {
            if (weaponModel != null)
            {
                if (isTwoHanded)
                {
                    // Check the player's current state and adjust the weapon's position
                    if (isAiming)
                    {
                        // Move to aiming position
                        weaponModel.transform.position = Vector3.Lerp(
                            weaponModel.transform.position,
                            thAimPos.transform.position,
                            Time.deltaTime * switchTime
                        );
                        weaponModel.transform.rotation = Quaternion.Lerp(
                            weaponModel.transform.rotation,
                            thAimPos.transform.rotation,
                            Time.deltaTime * switchTime
                        );
                    }
                    else
                    {
                        // Move to idle position
                        weaponModel.transform.position = Vector3.Lerp(
                            weaponModel.transform.position,
                            thIdlePos.transform.position,
                            Time.deltaTime * switchTime
                        );
                        weaponModel.transform.rotation = Quaternion.Lerp(
                            weaponModel.transform.rotation,
                            thIdlePos.transform.rotation,
                            Time.deltaTime * switchTime
                        );
                    }
                }
                else
                {
                    // For one-handed weapons, no movement is needed; maintain default position
                    weaponModel.transform.position = ohPos.transform.position;
                    weaponModel.transform.rotation = ohPos.transform.rotation;
                }
            }

            // Wait for the next frame
            yield return null;
        }




    }
}