using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using UnityEngine.Audio;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin, menuLose;
    // Settings Objects
    [SerializeField] GameObject menuSettings;
    [SerializeField] GameObject settingsActive;
    [SerializeField] GameObject inSetActive;
    // Audio Objects
    [Header("-----Audio-----")]
    [SerializeField] AudioSource aud;

    [SerializeField] AudioClip audWinMenu;
    [SerializeField][Range(0, 1)] float audWinVol;
    [SerializeField] AudioClip audLoseMenu;
    [SerializeField][Range(0, 1)] float audLoseVol;

    [SerializeField] GameObject menuAudio;
    [SerializeField] private TMP_Text volumeTextValue;
    [SerializeField] public Slider volumeSlider;
    [SerializeField] public float defaultVolume = 0.6f;
    [SerializeField] AudioMixer musicMixer;
    [SerializeField] AudioMixer sfxMixer;
    [SerializeField] public Slider sfxSlider;
    // Gameplay Objects
    [Header("-----Gameplay-----")]
    [SerializeField] GameObject menuGameplay;
    [SerializeField] private TMP_Text sensTextValue;
    [SerializeField] private Slider sensSlider;
    [SerializeField] private int defaultSen = 300;
    [SerializeField] private Toggle invertYToggle;
    // Graphics Objects
    [Header("-----Graphics-----")]
    [SerializeField] GameObject menuGraphics;
    [SerializeField] private TMP_Dropdown qltyDropdown;
    [SerializeField] private Toggle fullScrToggle;
    public TMP_Dropdown resDropDown;
    private Resolution[] resolutions;
    // Controls
    [Header("-----Controls-----")]
    [SerializeField] GameObject menuControl;
    // Main Menu
    [Header("-----Main Menu-----")]
    [SerializeField] GameObject mainConfrim;

    private int qualityLevel;
    private bool isFullScreen;
    private float brightnesslevel;
    private bool isInvertY;

    [Header("-----Count Text-----")]
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text ammoCountText;

    private HashSet<string> keyInventory = new HashSet<string>();

    [Header("-----Player-----")]
    public GameObject player;
    public Image playerHPBar;
    public Image playerStaminaBar;
    public Image playerConversionBar;
    public PlayerController playerScript;
    public GameObject playerSpawnPos;
    public GameObject playerDamageScreen;

    public ThrowObjects throwObjects;
    public Image Throwers;

    [Header("-----Friendly Setting-----")]
    [SerializeField][Range(1, 6)] public int maxFriendlies = 3;
    private List<IFriendly> friendlyUnits = new List<IFriendly>();

    [Header("-----Damage Effects-----")]
    public Volume dmgEffect;
    PostProcessVolume gmDmgEffectVol;
    Vignette gmDmgVignette;

    public bool isPaused;
    private bool gameEnded; //flag to indicate win/lose state
    private bool individualSet;

    float timeScaleOriG;
    public int enemyCount;
    public int ammoCount;

    public void ResetDmgScreen()
    {
        if (gmDmgVignette.enabled)
        {
            gmDmgVignette.enabled.Override(false);
        }
    }

    void Awake()
    {

        instance = this;
        timeScaleOriG = Time.timeScale;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        resolutions = Screen.resolutions;
        resDropDown.ClearOptions();

        List<string> resOptions = new List<string>();
        int currRes = 19;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string res = resolutions[i].width + " x " + resolutions[i].height;
            resOptions.Add(res);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currRes = i;
            }
        }


        resDropDown.AddOptions(resOptions);
        resDropDown.value = currRes;
        resDropDown.RefreshShownValue();


        gmDmgEffectVol = dmgEffect.GetComponent<PostProcessVolume>();
        gmDmgEffectVol.profile.TryGetSettings<Vignette>(out gmDmgVignette);
        gmDmgVignette.enabled.Override(false);
    }

    void Start()
    {
        if (throwObjects == null)
        {

            throwObjects = player.GetComponent<ThrowObjects>();
        }
        InitializeThrowers();

        // patch code in an attempt to clear unknown origin bug . 
        //ThrowObjects throwtalker = player.GetComponent<ThrowObjects>();
        //throwtalker.ResetThrow();
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolume", volumeSlider.value);
        //sensTextValue.text = MenuController.instance.mainSens.ToString("F0");
        //sensSlider.value = MenuController.instance.mainSens;
        //cameraController.camController.Sensitivity = MenuController.instance.mainSens;

        //sensTextValue.text = PlayerPrefs.GetInt("mainSens").ToString("F0");    }
    }

    void Update()
    {

        if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.P) && !gameEnded)
        {


            if (menuActive == null && settingsActive == null && inSetActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(IsPaused);



                ToggleSpinObjects(IsPaused);
            }
            else if (menuActive == menuPause && settingsActive == null && inSetActive == null)
            {
                stateUnpause();
                ToggleSpinObjects(true);

            }
            else if (menuActive == menuPause && settingsActive == menuSettings && inSetActive == null)
            {
                settingsActive.SetActive(false);
                settingsActive = null;
                stateUnpause();

            }
            else if (menuActive == menuPause && settingsActive == menuSettings && inSetActive != null)
            {
                inSetActive.SetActive(false);
                inSetActive = null;
                settingsActive.SetActive(false);
                settingsActive = null;
                stateUnpause();

            }
        }
    }

    public void resetMenu()
    {
        menuActive = null;
        gmDmgVignette.enabled.Override(false);
    }

    public bool IsPaused
    {
        get { return isPaused; }
        set
        {
            isPaused = !isPaused;
            menuPause.SetActive(IsPaused);
        }
    }

    public void statePause()
    {
        if (!gameEnded)
        {
            IsPaused = isPaused;
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

        }
    }

    public void stateUnpause()
    {
        IsPaused = isPaused;
        Time.timeScale = timeScaleOriG;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }

    }

    public void ResetTimeScale()
    {
        Time.timeScale = timeScaleOriG;
    }

    private void ToggleSpinObjects(bool enableSpin)
    {               // use findobjectsbytype as findobjectsoftype has depreciated in this version of unity 
        spin[] spinObjects = FindObjectsByType<spin>(FindObjectsSortMode.None);
        foreach (var spinObject in spinObjects)
        {
            spinObject.enabled = enableSpin;
        }
    }

    public void ammoUpdate(int ammoAmount)
    {
        ammoCount = Mathf.RoundToInt(ammoAmount);
        ammoCountText.text = ammoCount.ToString("F0");
    }

    public void AddKey(string keyID)
    {
        if (!keyInventory.Contains(keyID))
        {
            keyInventory.Add(keyID);
            Debug.Log($"Key {keyID} collected!");
        }
    }

    public bool UseKey(string keyID)
    {
        if (keyInventory.Contains(keyID))
        {
            keyInventory.Remove(keyID);
            Debug.Log($"Key {keyID} used!");
            return true;
        }
        Debug.LogWarning($"Key {keyID} not yet obtained!");
        return false;
    }

    public void InitializeThrowers()
    {
        UpdateThrowers(throwObjects.remainingThrows);
    }

    public void UpdateThrowers(int remainingThrows)
    {
        float fillAmount = Mathf.Clamp01((float)remainingThrows / throwObjects.GetTotalThrows());
        Throwers.fillAmount = fillAmount;
    }

    public void updateGameGoal(int amount)
    {
        enemyCount += amount;

        if (enemyCount > 0)
        {
            enemyCountText.text = enemyCount.ToString("F0");
            enemyCountText.gameObject.SetActive(true);
        }
        else
        {
            enemyCountText.gameObject.SetActive(false);
        }
    }

    public void showWinMenu()
    {
        gameEnded = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void youLose()
    {
        gameEnded = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuActive = menuLose;
        menuActive.SetActive(true);
        gmDmgVignette.enabled.Override(false);
    }

    public void respawnManager()
    {
        gameEnded = false;
        Time.timeScale = timeScaleOriG;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive = menuLose;
        menuActive.SetActive(false);

        playerScript.Revive();

    }

    // Settings methods
    public void setMenu()
    {
        menuActive.SetActive(false);
        settingsActive = menuSettings;
        settingsActive.SetActive(true);
    }

    public void settingsBack()
    {
        settingsActive.SetActive(false);
        settingsActive = null;
        menuActive.SetActive(true);
    }

    public bool IndividualSet
    {
        get { return individualSet; }
        set
        {
            individualSet = !individualSet;
            inSetActive.SetActive(IndividualSet);
        }
    }

    // Audio
    public void audioMenu()
    {
        settingsActive.SetActive(false);
        inSetActive = menuAudio;
        inSetActive.SetActive(true);
    }

    public void setVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeTextValue.text = volume.ToString("F1");
    }

    public void volumeApply()
    {
        PlayerPrefs.SetFloat("musicVolume", AudioListener.volume);
        if (inSetActive != null)
        {
            inSetActive.SetActive(false);
            inSetActive = null;
            settingsActive.SetActive(true);
        }
    }

    public void inSetBack()
    {
        inSetActive.SetActive(false);
        inSetActive = null;
        settingsActive.SetActive(true);
    }

    public void resetDefault(string menuType)
    {
        if (menuType == "Audio")
        {

            musicMixer.SetFloat("MusicVolume", MathF.Log10(volumeSlider.value) * 30f);
            volumeSlider.value = 0.6f;
            //volumeTextValue.text = volumeSlider.value.ToString("F1");
            sfxMixer.SetFloat("SFXVolume", Mathf.Log10(sfxSlider.value) * 30f);
            sfxSlider.value = 0.6f;
            volumeApply();
            if (inSetActive != null)
            {
                inSetActive.SetActive(false);
                inSetActive = null;
                settingsActive.SetActive(true);
            }
        }

        if (menuType == "Gameplay")
        {
            sensTextValue.text = defaultSen.ToString("F0");
            sensSlider.value = defaultSen;
            invertYToggle.isOn = false;
            gameplayApply();
        }

        if (menuType == "Graphics")
        {
            //Reset brightness
            qltyDropdown.value = 1;
            QualitySettings.SetQualityLevel(1);
            fullScrToggle.isOn = true;
            Screen.fullScreen = true;

            Resolution currRes = Screen.currentResolution;
            Screen.SetResolution(currRes.width, currRes.height, Screen.fullScreen);
            resDropDown.value = resolutions.Length;
            graphicsApply();
        }
    }

    // Gameplay
    public void gameplayMenu()
    {
        settingsActive.SetActive(false);
        inSetActive = menuGameplay;
        inSetActive.SetActive(true);
    }

    public void setSensitivity(float sensitivity)
    {

        cameraController.camController.Sensitivity = (int)sensitivity;

        sensTextValue.text = sensitivity.ToString("F0");
    }

    public void gameplayApply()
    {
        if (invertYToggle.isOn)
        {
            cameraController.camController.InvertY = true;
        }
        else
        {
            cameraController.camController.InvertY = false;
        }
        inSetActive.SetActive(false);
        inSetActive = null;
        settingsActive.SetActive(true);
    }

    public bool IsInvertY
    {
        get { return isInvertY; }
        set
        {
            isInvertY = !isInvertY;
        }
    }

    public void setInvertY(bool invertY)
    {
        isInvertY = invertY;
    }

    // Graphics
    public void graphMenu()
    {
        settingsActive.SetActive(false);
        inSetActive = menuGraphics;
        inSetActive.SetActive(true);
    }

    public bool IsFullScreen
    {
        get { return isFullScreen; }
        set
        {
            isFullScreen = !isFullScreen;
        }
    }

    public void setFullScreen(bool fullScreen)
    {
        isFullScreen = fullScreen;
    }

    public void setQuality(int qualityIndex)
    {
        qualityLevel = qualityIndex;
    }

    public void graphicsApply()
    {
        //brightness
        PlayerPrefs.SetInt("masterQuality", qualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel);

        PlayerPrefs.SetInt("masterFullSCreen", (IsFullScreen ? 1 : 0));
        Screen.fullScreen = IsFullScreen;
        setResolution(resDropDown.value);
        inSetBack();
    }

    // Controls
    public void cntrlMenu()
    {
        settingsActive.SetActive(false);
        inSetActive = menuControl;
        inSetActive.SetActive(true);
    }

    public void setResolution(int resIndex)
    {
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // Main Menu
    public void mainMenuRtn()
    {
        menuActive.SetActive(false);
        settingsActive = mainConfrim;
        settingsActive.SetActive(true);
    }


    public void RegisterFriendly(IFriendly friendly)
    {
        if (!friendlyUnits.Contains(friendly))
        {
            if (friendlyUnits.Count >= maxFriendlies)
            {
                IFriendly oldestFriendly = friendlyUnits[0];
                oldestFriendly.Die();
            }
            friendlyUnits.Add(friendly);
            UpdateFollowOffsets();
        }
    }

    public void RemoveFriendly(IFriendly friendly)
    {
        if (friendlyUnits.Contains(friendly))
        {
            friendlyUnits.Remove(friendly);
            UpdateFollowOffsets();
        }
    }

    public int GetFriendlyIndex(IFriendly friendly)
    {
        return friendlyUnits.IndexOf(friendly);
    }

    public int GetFriendlyCount()
    {
        return friendlyUnits.Count;
    }

    private void UpdateFollowOffsets()
    {
        int totalFriendlies = GetFriendlyCount();
        for (int i = 0; i < totalFriendlies; i++)
        {
            IFriendly friendly = friendlyUnits[i];
            if (friendly is FriendlyAI friendlyAI)
            {
                friendlyAI.AssignFollowOffset(i, totalFriendlies);
            }
        }
    }
}

