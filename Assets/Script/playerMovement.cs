using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class playerMovement : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float jumpForce;
    [SerializeField] private Rigidbody2D rb;

    [Header("Audio của Player")]
    [SerializeField] private AudioSource sfxAudioSource; // Jump & Die
    [SerializeField] private AudioSource backgroundAudioSource; // Nhạc nền
    [SerializeField] private AudioClip jumpSPX;
    [SerializeField] private AudioClip dieSPX;
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Audio Mixer Group")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup backgroundGroup;

    [HideInInspector] public bool isPaused = false; // <-- Dùng cho Pause Menu

    private bool _batDauGame = false;
    private bool _chamVaoMatDat = false;
    private bool _playerChet = false;
    private bool daPhatNhacNen = false;

    void Start()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        if (sfxGroup != null)
            sfxAudioSource.outputAudioMixerGroup = sfxGroup;
        if (backgroundGroup != null)
            backgroundAudioSource.outputAudioMixerGroup = backgroundGroup;

        float bgVolume = PlayerPrefs.GetFloat("BackgroundVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        bgVolume = Mathf.Clamp(bgVolume, 0.001f, 1f);
        sfxVolume = Mathf.Clamp(sfxVolume, 0.001f, 1f);

        if (backgroundGroup != null && backgroundGroup.audioMixer != null)
        {
            backgroundGroup.audioMixer.SetFloat("BackgroundVolume", Mathf.Log10(bgVolume) * 20);
            backgroundGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        }

        Debug.Log($"Áp dụng âm lượng: BG = {bgVolume}, SFX = {sfxVolume}");
    }

    void Update()
    {
        if (isPaused) return; // ⛔ Không xử lý gì khi đang pause

        bool isJumpButtonPressed = false;
        bool isCrouchButtonPressed = false;

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    if (touch.position.x < Screen.width / 2)
                        isJumpButtonPressed = true;
                    else
                        isCrouchButtonPressed = true;
                }
                else if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
                {
                    if (touch.position.x >= Screen.width / 2)
                        isCrouchButtonPressed = true;
                }
            }
        }

        if (!_playerChet)
        {
            if (isJumpButtonPressed)
            {
                if (_batDauGame && _chamVaoMatDat)
                {
                    Jump();
                }
                else
                {
                    _batDauGame = true;
                    GameManager.Instance.batDauGame = true;

                    if (!daPhatNhacNen)
                    {
                        backgroundAudioSource.clip = backgroundMusic;
                        backgroundAudioSource.loop = true;
                        backgroundAudioSource.Play();
                        daPhatNhacNen = true;
                    }
                }
            }
        }

        playerAnimator.SetBool("Batdaugame", _batDauGame);
        playerAnimator.SetBool("isCrouching", isCrouchButtonPressed && _chamVaoMatDat && !isJumpButtonPressed);
        playerAnimator.SetBool("playerChet", _playerChet);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ground"))
        {
            _chamVaoMatDat = true;
        }
        else if (other.gameObject.CompareTag("trap"))
        {
            _playerChet = true;
            GameManager.Instance.ketThucGame = true;
            GameManager.Instance.ShowEndScreen();

            backgroundAudioSource.Stop();
            sfxAudioSource.PlayOneShot(dieSPX);
        }
    }

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce);
        _chamVaoMatDat = false;
        sfxAudioSource.PlayOneShot(jumpSPX);
    }
}
