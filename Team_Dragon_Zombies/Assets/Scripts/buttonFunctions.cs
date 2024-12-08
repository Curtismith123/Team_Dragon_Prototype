using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void resume()
    {
        gameManager.instance.stateUnpause();
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Settings Buttons
    public void settingsMenu()
    {
        gameManager.instance.setMenu();
    }

    public void setBack()
    {
        gameManager.instance.settingsBack();
    }

    // Audio
    public void soundMenu()
    {
        gameManager.instance.audioMenu();
    }

    public void audioApply()
    {
        gameManager.instance.volumeApply();
    }

    public void audioReset()
    {
        gameManager.instance.resetDefault("Audio");
    }

    public void indiviualBack()
    {
        gameManager.instance.inSetBack();
    }

    // Gameplay
    public void gmPlayApply()
    {
        gameManager.instance.gameplayApply();
    }

    public void resetGmPlay()
    {
        gameManager.instance.resetDefault("Gameplay");
    }

    // Graphics
    public void graphicsMenu()
    {
        gameManager.instance.graphMenu();
    }

    public void graphAppply()
    {
        gameManager.instance.graphicsApply();
    }
    public void resetGraphics()
    {
        gameManager.instance.resetDefault("Graphics");
    }

    // Controls
    public void controlMenu()
    {
        gameManager.instance.cntrlMenu();
    }

}
