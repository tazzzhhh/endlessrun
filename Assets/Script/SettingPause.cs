using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingPause : MonoBehaviour
{
    //Hàm về màn hình dừng game
    [SerializeField] GameObject pauseMenu;

    //Dòng hàm về âm thanh
    [Header("Âm Thanh")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider backgroundSlider;
    [SerializeField] private Slider spxSlider;

    private const string BackgroundVolumeKey = "BackgroundVolume";
    private const string SFXVolumeKey = "SFXVolume";

    //Hàm chỉ định player đứng yên - pause game
    [Header("Chỉnh Player đứng yên khi pause")]
    [SerializeField] private playerMovement player;

    void Start()
    {
        LoadVolumeSettings();
    }

    //Ứng dụng Pause
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        if (player != null)
            player.isPaused = true;
    }

    //Hàm xử lí nút bấm Resume(Trở lại game)
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        if (player != null)
            player.isPaused = false;
    }

    //Hàm xử lý nút bấm restart
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    //Hàm xử lý nút bấm BackMenu(trở lại màn hình chính game)
    public void BackMenu()
    {
        SceneManager.LoadScene("LoadingScene");
        LoadingScene.sceneToLoad = "MainMenu";
        Time.timeScale = 1;
    }

    //Hàm xử lý chỉnh nhạc nền của game(backgroundVolume)
    public void SetBackgroundVolume()
    {
        float volume = backgroundSlider.value;
        audioMixer.SetFloat(BackgroundVolumeKey, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(BackgroundVolumeKey, volume); // Lưu âm lượng background đã chỉnh
    }

    //Hàm xử lý chỉnh âm thanh nền của game(spxVolume)
    public void SetSFXVolume()
    {
        float volume = spxSlider.value;
        audioMixer.SetFloat(SFXVolumeKey, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(SFXVolumeKey, volume); // Lưu âm lượng sfx đã chỉnh
    }

    //Thêm dòng lệnh để load setting âm thanh đã chỉnh 
    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(BackgroundVolumeKey))
        {
            float backgroundVolume = PlayerPrefs.GetFloat(BackgroundVolumeKey);
            backgroundSlider.value = backgroundVolume;
            audioMixer.SetFloat(BackgroundVolumeKey, Mathf.Log10(backgroundVolume) * 20);
        }
        else
        {
            //Nếu như không chỉnh -> giá trị của setting sẽ về mặc định
            backgroundSlider.value = 0.75f; //Ví dụ 
            audioMixer.SetFloat(BackgroundVolumeKey, Mathf.Log10(0.75f) * 20);
        }

        if (PlayerPrefs.HasKey(SFXVolumeKey))
        {
            float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey);
            spxSlider.value = sfxVolume;
            audioMixer.SetFloat(SFXVolumeKey, Mathf.Log10(sfxVolume) * 20);
        }
        else
        {
            //Nếu như không chỉnh -> giá trị của setting sẽ về mặc định
            spxSlider.value = 0.75f; // Ví dụ
            audioMixer.SetFloat(SFXVolumeKey, Mathf.Log10(0.75f) * 20);
        }
    }
}