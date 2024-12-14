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
            PlayerPrefs.SetFloat("musicVolume", mSlider.value);
    }

    private void Start()
    {
            mSlider.value = PlayerPrefs.GetFloat("musicVolume", mSlider.value);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
