using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class SettingsButton : MonoBehaviour
{
  #region Private Fields
    [SerializeField]
    Button settingsCog;

    [SerializeField]
    Button closeX;

    [SerializeField]
    Slider pushThresholdValue;

    [SerializeField]
    TMP_Text pushThresholdSliderValueText;

    [SerializeField]
    Slider multiplierValue;

    [SerializeField]
    TMP_Text multiplierSliderValueText;

    [SerializeField]
    GameObject settingsPanel;

    [SerializeField]
    Toggle backgroundToggle;

    [SerializeField]
    Slider backgroundVolume;

    [SerializeField]
    Slider soundEffectVolume;

    [SerializeField]
    Toggle soundEffectToggle;

    [SerializeField]
    GameObject backgroundMusicGO;

    [SerializeField]
    GameObject soundEffectGO;

    AudioSource soundEffectAudio;
    AudioSource backgroundAudio;
    RectTransform rectTransform;
    RectTransform settingsPanelRectTransform;

  #endregion

  #region Public Fields
  #endregion

  #region C# Events
  #endregion

  #region Public Methods
  #endregion

  #region Unity Methods
    void Start()
    {
        soundEffectAudio = soundEffectGO.GetComponent<AudioSource>();
        backgroundAudio = backgroundMusicGO.GetComponent<AudioSource>();


        settingsCog.onClick.AddListener(OpenSettingsMenu);
        closeX.onClick.AddListener(CloseSettingsMenu);

        print("Dyna Max" + CubeController.dynamicMultiplierMax);
        multiplierValue.onValueChanged.AddListener(ModifyMultiplerValue);
        
        backgroundVolume.onValueChanged.AddListener(ModifyMusicVolume);
        backgroundToggle.onValueChanged.AddListener(ToggleMusicSound);

        soundEffectVolume.onValueChanged.AddListener(ModifySoundEffectVolume);
        soundEffectToggle.onValueChanged.AddListener(ToggleEffectSound);

        print("Slide Threshold " + CubeController.slideThreshold);
        pushThresholdValue.onValueChanged.AddListener(ModifySlideThrehold);
        pushThresholdValue.minValue = 0;
        pushThresholdValue.maxValue = CubeController.slideThreshold * 2;

        multiplierValue.maxValue = CubeController.dynamicMultiplierMax * 1.5f;
        
        if (SystemInfo.deviceType != DeviceType.Handheld)
        {
            multiplierValue.minValue = 10/2;
        }
        else
        {
            multiplierValue.minValue = 2/2;
        }
        multiplierValue.value = CubeController.dynamicMultiplierMax;
        pushThresholdValue.value = CubeController.slideThreshold;

        rectTransform = GetComponent<RectTransform>();
        settingsPanelRectTransform = settingsPanel.GetComponent<RectTransform>();

        backgroundVolume.value = backgroundAudio.volume;
        soundEffectVolume.value = soundEffectAudio.volume;
    }

    void Update()
    {
        multiplierSliderValueText.text = multiplierValue.value.ToString();
        pushThresholdSliderValueText.text = pushThresholdValue.value.ToString();
       
    }
  #endregion

  #region Private Methods
    private void OpenSettingsMenu()
    {
        print("Menu X pre move: " + settingsPanelRectTransform.position.x);
        settingsPanel.SetActive(true);
        rectTransform.DOMoveX(-51f, .2f);
        settingsPanelRectTransform.DOMoveX(200f, .2f);
        print("Menu X post move: " + settingsPanelRectTransform.position.x);

    }

    private void CloseSettingsMenu()
    {
        settingsPanelRectTransform.DOMoveX(-200f, .2f);
        rectTransform.DOMoveX(51f, .2f);
        StartCoroutine(CountDownToDisable());
    }

    private void ToggleMusicSound(bool isMuted)
    {
        backgroundAudio.mute = isMuted;
    }

    private void ToggleEffectSound(bool isMuted)
    {
        soundEffectAudio.mute = isMuted;
    }

    private void ModifySoundEffectVolume(float sliderVal)
    {
        soundEffectAudio.volume = sliderVal;
    }

    private void ModifyMusicVolume(float sliderVal)
    {
        backgroundAudio.volume = sliderVal;
    }
    private void ModifySlideThrehold(float sliderVal)
    {
        CubeController.slideThreshold = sliderVal;
    }

    private void ModifyMultiplerValue(float sliderVal)
    {
        CubeController.dynamicMultiplierMax = sliderVal;
    }

    private IEnumerator CountDownToDisable()
    {
        yield return Utilities.HelperFunctions.Timer(.3f);
        settingsPanel.SetActive(false);
    }
  #endregion
}
