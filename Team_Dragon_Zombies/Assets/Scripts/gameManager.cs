using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin, menuLose;
    [SerializeField] GameObject spinObject;

    public Image playerHPBar;
    public GameObject playerDamageScreen;

    public GameObject player;
    public PlayerController playerScript;

    private bool isPaused;

    float timeScaleOG;

    int enemyCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        timeScaleOG = Time.timeScale;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(IsPaused);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    // Setting up getters and sitters for isPaused variable
    public bool IsPaused
    {
        get { return isPaused; }
        set
        {
            isPaused = !isPaused;
            menuPause.SetActive(IsPaused);
        }
    }

    // Pause State
    public void statePause()
    {
        IsPaused = isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Unpause state
    public void stateUnpause()
    {
        IsPaused = isPaused;
        Time.timeScale = timeScaleOG;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    // Update game goal
    public void updateGameGoal(int amount)
    {
        enemyCount += amount;

        // YOU WIN
        if (enemyCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    // You lose menu
    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}
