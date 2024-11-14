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

    [SerializeField] TMP_Text enemyCountText;

    public Image playerHPBar;
    public GameObject playerDamageScreen;

    public GameObject player;
    public PlayerController playerScript;

    private bool isPaused;
    private bool gameEnded; //flag to indicate win/lose state
    private bool individualSet;

    float timeScaleOriG;
    public int enemyCount;

    void Awake()
    {
        instance = this;
        timeScaleOriG = Time.timeScale;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
    }

    void Update()
    {

        if (Input.GetButtonDown("Cancel") && !gameEnded)
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(IsPaused);

                ToggleSpinObjects(IsPaused);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
                ToggleSpinObjects(true);
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


    public void updateGameGoal(int amount)
    {
        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");

        if (enemyCount <= 0)
        {
            showWinMenu();
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
        //StartCoroutine(confirmBox());
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
    }

    //public IEnumerator confirmBox()
    //{
    //    confirmPromp.SetActive(true);
    //    yield return new WaitForSeconds(1);
    //    confirmPromp.SetActive(false);
    //}
    
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

    public void gameplayBack()
    {
        inSetActive.SetActive(false);
        inSetActive = null;
        settingsActive.SetActive(true);
    }

    public void gameplayDefault()
    {

    }
}