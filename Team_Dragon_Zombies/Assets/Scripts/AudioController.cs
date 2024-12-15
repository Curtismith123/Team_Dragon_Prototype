using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] string mVolume = "MasterVolume";
    [SerializeField] AudioMixer mMixer;
    [SerializeField] Slider mSlider;
    [SerializeField] float mMult = 30f;


    private void Awake()
    {
        mSlider.onValueChanged.AddListener(SliderValChange);
    }

    private void SliderValChange(float val)
    {
        mMixer.SetFloat(mVolume, MathF.Log10(val) * mMult);
    }

    private void OnDisable()
    {
        if (mVolume == "MusicVolume")
        {
            PlayerPrefs.SetFloat("MusicVolume", mSlider.value);
        }
        if (mVolume == "SFXVolume")
        {
            PlayerPrefs.SetFloat("SFXVolume", mSlider.value);
        }
    }

    private void Start()
    {
        if (mVolume == "MusicVolume")
        {
            mSlider.value = PlayerPrefs.GetFloat("MusicVolume", mSlider.value);
        }
        if (mVolume == "SFXVolume")
        {
            mSlider.value = PlayerPrefs.GetFloat("SFXVolume", mSlider.value);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
