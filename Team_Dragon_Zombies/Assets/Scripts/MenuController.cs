using UnityEngine;
//using static UnityEditorInternal.ReorderableList;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    // Audio Objects
    //[Header("-----Audio-----")]
    //[SerializeField] AudioSource aud;

    //[SerializeField] AudioClip audWinMenu;
    //[SerializeField][Range(0, 1)] float audWinVol;
    //[SerializeField] AudioClip audLoseMenu;
    //[SerializeField][Range(0, 1)] float audLoseVol;

    [SerializeField] GameObject menuSettings;
    [Header("-----Audio-----")]
    //[SerializeField] GameObject menuAudio;
    [SerializeField] private TMP_Text volumeTextValue;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private float defaultVolume = 1.0f;
    // Gameplay Objects
    [Header("-----Gameplay-----")]
    [SerializeField] GameObject menuGameplay;
    [SerializeField] public TMP_Text mainSensTextValue;
    [SerializeField] public Slider sensSlider;
    [SerializeField] private int defaultSen = 300;
    public int mainSens = 300;
    [SerializeField] private Toggle invertYToggle;
    // Graphics Objects
    [Header("-----Graphics-----")]
    [SerializeField] GameObject menuGraphics;
    [SerializeField] private TMP_Dropdown qltyDropdown;
    [SerializeField] private Toggle fullScrToggle;
    public TMP_Dropdown resDropDown;
    private Resolution[] resolutions;

    private int qualityLevel;
    private bool isFullScreen;
    private float brightnesslevel;

    // Main Menu
    [Header("-----New Game-----")]
    public string newGameLevel;
    private string loadLevel;
    [Header("-----Credits----")]
    public GameObject credits;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        qltyDropdown.value = 5;
        QualitySettings.SetQualityLevel(5);
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

        resDropDown.AddOptions(resOptions);
        resDropDown.value = 38;
        resDropDown.RefreshShownValue();
        menuSetResolution(38);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Audio

    public void menuSetVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeTextValue.text = volume.ToString("F1");
    }

    public void menuVolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
    }

    public void menuResetDefault(string menuType)
    {
        if (menuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("F1");
            menuVolumeApply();
        }

        if (menuType == "Gameplay")
        {
            mainSensTextValue.text = defaultSen.ToString("F0");
            sensSlider.value = defaultSen;
            invertYToggle.isOn = false;
            menuGameplayApply();
        }

        if (menuType == "Graphics")
        {
            //Reset brightness
            qltyDropdown.value = 5;
            QualitySettings.SetQualityLevel(5);
            fullScrToggle.isOn = true;
            Screen.fullScreen = true;

            Resolution currRes = Screen.currentResolution;
            Screen.SetResolution(currRes.width, currRes.height, Screen.fullScreen);
            resDropDown.value = resolutions.Length;
            menuGraphicsApply();
        }
    }

    // Gameplay

    public void menuSetSensitivity(float sensitivity)
    {

        //cameraController.camController.Sensitivity = (int)sensitivity;
        mainSens = Mathf.RoundToInt(sensitivity);
        mainSensTextValue.text = sensitivity.ToString("F0");
    }

    public void menuGameplayApply()
    {

        if (invertYToggle.isOn)
        {
            //cameraController.camController.InvertY = true;
            PlayerPrefs.SetInt("masterInvertY", 1);
        }
        else
        {
            //cameraController.camController.InvertY = false;
            PlayerPrefs.SetInt("masterInvertY", 0);
        }

        PlayerPrefs.SetFloat("masterSensitivity", mainSens);
    }

    // Graphics

    public bool IsFullScreen
    {
        get { return isFullScreen; }
        set
        {
            isFullScreen = !isFullScreen;
        }
    }

    public void menuSetFullScreen(bool fullScreen)
    {
        isFullScreen = fullScreen;
    }

    public void menuSetQuality(int qualityIndex)
    {
        qualityLevel = qualityIndex;
    }

    public void menuGraphicsApply()
    {
        
        PlayerPrefs.SetInt("masterQuality", qualityLevel);
        QualitySettings.SetQualityLevel(qltyDropdown.value);

        PlayerPrefs.SetInt("masterFullSCreen", (IsFullScreen ? 1 : 0));
        Screen.fullScreen = IsFullScreen;
        menuSetResolution(resDropDown.value);
    }

    public void menuSetResolution(int resIndex)
    {
        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // Main Menu

    public void newGameYes()
    {
        SceneManager.LoadScene(newGameLevel);
    }

    public void mainAudioApply()
    {
        menuVolumeApply();
    }

    public void mainAudioReset()
    {
        menuResetDefault("Audio");
    }

}
