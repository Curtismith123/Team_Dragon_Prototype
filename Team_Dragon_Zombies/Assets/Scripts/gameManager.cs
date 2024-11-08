using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin, menuLose;
    [SerializeField] GameObject spinObject;
    [SerializeField] TMP_Text enemyCountText;

    public Image playerHPBar;
    public GameObject playerDamageScreen;

    public GameObject player;
    public PlayerController playerScript;

    private bool isPaused;
    private bool gameEnded; //flag to indicate win/lose state

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
                spinObject.GetComponent<spin>().enabled = true;
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
}