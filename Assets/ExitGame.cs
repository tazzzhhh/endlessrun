using UnityEngine;

public class ExitGame : MonoBehaviour
{
    [SerializeField] private GameObject exitPanel; // Kéo ExitPanel vào Inspector

    void Start()
    {
        // Đảm bảo panel tắt lúc bắt đầu game
        if (exitPanel != null)
            exitPanel.SetActive(false);
    }

    void Update()
    {
        // Chỉ chạy khi người chơi bấm Back (Android) hoặc Escape (PC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exitPanel != null)
            {
                bool isActive = exitPanel.activeSelf;
                exitPanel.SetActive(!isActive);
            }
        }
    }

    // Nhấn nút YES
    public void OnYesButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        // Test trong Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Nhấn nút NO
    public void OnNoButton()
    {
        if (exitPanel != null)
            exitPanel.SetActive(false);
    }
}
