using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuGame : MonoBehaviour
{
    public GameObject mainMenu;

    //Màn hình hướng dẫn game
    [SerializeField] GameObject tutorialScreen;

    [Header("Tutorial Content")]
    [SerializeField] private Image pageImage;
    [SerializeField] private Sprite[] tutorialPages;

    [SerializeField] private TextMeshProUGUI textTitle;
    [SerializeField] private string[] tutorialInformation;


    //Phần Setting Game
    [Header("Setting game")]
    [SerializeField] GameObject settingPanel;
    [SerializeField] private TMP_Dropdown graphicsDropdown;
    [SerializeField] private TMP_Dropdown fpsDropdown;
    [SerializeField] private TMP_Dropdown languageDropdown;

    //Setting cài ngôn ngữ Anh - Việt
    [Header("UI Language Text")]
    [SerializeField] private TextMeshProUGUI settingName;
    [SerializeField] private TextMeshProUGUI languageLabel;
    [SerializeField] private TextMeshProUGUI fpsLabel;
    [SerializeField] private TextMeshProUGUI qualityLabel;

    //Setting chỉnh cảnh scene sáng - tối
    [Header("Setting Change D-N")]
    [SerializeField] private TMP_Dropdown changeSceneDropdown;


    private int currentPage = 0; //Số trang

    void Start()
    {
        // Đảm bảo rằng các slider được gán giá trị đúng trước khi SetVolume được gọi
        // và PlayerPrefs đã được load.
        InitGraphicsDropdown();
        InitFPSDropdown();
        InitLanguageDropdown();
        InitChangeSceneDropdown(); 
  
    }

    private void InitChangeSceneDropdown()
    {
        // Lấy dữ liệu đã lưu, mặc định = Day (1)
        int startDay = PlayerPrefs.GetInt("StartWithDay", 1);

        // Set giá trị cho Dropdown (0 = Day, 1 = Night)
        changeSceneDropdown.value = (startDay == 1) ? 0 : 1;

        // Apply ngay lúc start
        ApplyChangeScene(changeSceneDropdown.value);

        // Gắn sự kiện khi đổi option
        changeSceneDropdown.onValueChanged.AddListener(OnChangeSceneChanged);
    }

    private void InitGraphicsDropdown()
    {
        int saveQuality = PlayerPrefs.GetInt("QualitySetting", QualitySettings.GetQualityLevel());
        graphicsDropdown.value = saveQuality;
        QualitySettings.SetQualityLevel(saveQuality);
    }

    private void InitFPSDropdown()
    {
        int savedFPS = PlayerPrefs.GetInt("FPSSetting", 60);
        int index = savedFPS switch
        {
            30 => 0,
            60 => 1,
            90 => 2,
            120 => 3,
            _ => 1
        };
        fpsDropdown.value = index;
        Application.targetFrameRate  = savedFPS;
        Debug.Log("FPS hiện tại: " + Application.targetFrameRate);
    }


    private void InitLanguageDropdown()
    {
        int langIndex = PlayerPrefs.GetInt("LanguageSetting", 0);
        languageDropdown.value = langIndex;
        ApplyLanguage(langIndex);
    }

    public void SetQualityLevel()
    {
        int index = graphicsDropdown.value;
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualitySetting", index);
    }

    public void SetFPSLevel()
    {
        int index = fpsDropdown.value;
        int fps = 60;
        switch (index)
        {
            case 0: fps = 30; break;
            case 1: fps = 60; break;
            case 2: fps = 90; break;
            case 3: fps = 120; break;
        }
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("FPSSetting", fps);
        Debug.Log("FPS Hiện Tại: " + Application.targetFrameRate);
    }

    public void SetLanguage()
    {
        int index = languageDropdown.value;
        ApplyLanguage(index);
        PlayerPrefs.SetInt("LanguageSetting", index);
    }

    private void ApplyLanguage(int index)
    {
        // Mô phỏng hành vi đổi ngôn ngữ
        if (index == 0)
        {
            Debug.Log("Language changed to: English");
            if (settingName != null) settingName.text = "SETTING";
            if (qualityLabel != null) qualityLabel.text = "QUALITY";
            if (fpsLabel != null) fpsLabel.text = "FPS SETTING";
            if (languageLabel != null) languageLabel.text = "LANGUAGE";

        }
        else
        {
            Debug.Log("Ngôn ngữ đã chuyển sang: Tiếng Việt");
            // Gán text tiếng Việt
            if (settingName != null) settingName.text = "CÀI ĐẶT";
            if (qualityLabel != null) qualityLabel.text = "CHấT LƯỢNG";
            if (fpsLabel != null) fpsLabel.text = "CHỈNH FPS";
            if (languageLabel != null) languageLabel.text = "NGÔN NGỮ";
        }
    }

    public void OnChangeSceneChanged(int index)
    {
        ApplyChangeScene(index);

        // Lưu lại lựa chọn (0 = Night, 1 = Day)
        PlayerPrefs.SetInt("StartWithDay", index == 0 ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyChangeScene(int index)
    {
        if (index == 0)
        {
            Debug.Log("Game sẽ bắt đầu ở cảnh: DAY");
            gameData.startWithDay = true; // biến static để GameManager đọc
        }
        else
        {
            Debug.Log("Game sẽ bắt đầu ở cảnh: NIGHT");
            gameData.startWithDay = false;
        }
    }


    public void OpenSettings()
    {
        settingPanel.SetActive(true);
        mainMenu.transform.Find("Start").gameObject.SetActive(false);
        mainMenu.transform.Find("Setting").gameObject.SetActive(false);
        mainMenu.transform.Find("Tutorial").gameObject.SetActive(false);

    }

    public void CloseSettings()
    {
        settingPanel.SetActive(false);
        mainMenu.transform.Find("Start").gameObject.SetActive(true);
        mainMenu.transform.Find("Setting").gameObject.SetActive(true);
        mainMenu.transform.Find("Tutorial").gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        // Đảm bảo tên Scene "ScenePlay" trùng khớp chính xác trong Build Settings
        SceneManager.LoadScene("LoadingScene");
        LoadingScene.sceneToLoad = "ScenePlay";
        Time.timeScale = 1;
    }

    public void OpenTutorial()
    {
        currentPage = 0;
        tutorialScreen.SetActive(true);
        mainMenu.transform.Find("Start").gameObject.SetActive(false);
        mainMenu.transform.Find("Setting").gameObject.SetActive(false);
        mainMenu.transform.Find("Tutorial").gameObject.SetActive(false);
        UpdatePage();
        Debug.Log("Đã mở tutorial");
    }

    public void CloseTutorial()
    {
        tutorialScreen.SetActive(false);
        mainMenu.transform.Find("Start").gameObject.SetActive(true);
        mainMenu.transform.Find("Setting").gameObject.SetActive(true);
        mainMenu.transform.Find("Tutorial").gameObject.SetActive(true);
    }

    public void NextPage()
    {
        if (currentPage < tutorialPages.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    public void UpdatePage()
    {
        // Kiểm tra null và độ dài mảng để tránh lỗi IndexOutOfRangeException
        if (pageImage != null && tutorialPages.Length > 0 && currentPage < tutorialPages.Length)
            pageImage.sprite = tutorialPages[currentPage];

        if (textTitle != null && tutorialInformation.Length > 0 && currentPage < tutorialInformation.Length)
            textTitle.text = tutorialInformation[currentPage];
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}