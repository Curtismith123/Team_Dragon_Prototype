using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
    [SerializeField] GameObject menuAudio;
    [SerializeField] private TMP_Text volumeTextValue;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private float defaultVolume = 1.0f;
    // Gameplay Objects
    [SerializeField] GameObject menuGameplay;
    [SerializeField] private TMP_Text sensTextValue;
    [SerializeField] private Slider sensSlider;
    [SerializeField] private int defaultSen = 300;
    [SerializeField] private Toggle invertYToggle;
    // Graphics Objects
    [SerializeField] GameObject menuGraphics;
    [SerializeField] private TMP_Dropdown qltyDropdown;
    [SerializeField] private Toggle fullScrToggle;
    public TMP_Dropdown resDropDown;
    private Resolution[] resolutions;

    private int qualityLevel;
    private bool isFullScreen;
    private float brightnesslevel;

    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text ammoCountText;

    public GameObject player;
    public Image playerHPBar;
    public Image playerStaminaBar;
    public PlayerController playerScript;
    public GameObject playerDamageScreen;

    public ThrowObjects throwObjects;
    public Image Throwers;


    private bool isPaused;
    private bool gameEnded; //flag to indicate win/lose state
    private bool individualSet;

    float timeScaleOriG;
    public int enemyCount;
    public int ammoCount;


    void Awake()
    {
        instance = this;
        timeScaleOriG = Time.timeScale;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();

        resolutions = Screen.resolutions;
        resDropDown.ClearOptions();

        List<string> resOptions = new List<string>();
        int currRes = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string res = resolutions[i].width + " x " + resolutions[i].height;
            resOptions.Add(res);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currRes = i;
            }
        }

        throwObjects = player.GetComponent<ThrowObjects>();

        resDropDown.AddOptions(resOptions);
        resDropDown.value = currRes;
        resDropDown.RefreshShownValue();

    }

    void Start()
    {
        InitializeThrowers();
    }

    void Update()
    {

        if (Input.GetButtonDown("Cancel") && !gameEnded)
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

    private void ToggleSpinObjects(bool enableSpin)
    {
        spin[] spinObjects = FindObjectsOfType<spin>();
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
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        inSetActive.SetActive(false);
        inSetActive = null;
        settingsActive.SetActive(true);
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
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("F1");
            volumeApply();
            inSetActive.SetActive(false);
            inSetActive = null;
            settingsActive.SetActive(true);
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

    public void setResolution(int resIndex)
    {
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}