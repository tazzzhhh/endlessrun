using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements; // QUAN TRỌNG để dùng Light2D

public class GameManager : MonoBehaviour
{
    //Phần quản lý nền di chuyển
    [SerializeField] private MeshRenderer groundMeshRenderer;
    [SerializeField] private MeshRenderer backgroundMeshRenderer;

    public static GameManager Instance;

    //Trạng thái của game
    [HideInInspector] public bool batDauGame = false;
    [HideInInspector] public bool ketThucGame = false;

    //Cài đặt tốc độ của game 
    [Header("Speeding của game")]
    public float startingSpeed = 5f; //Tốc độ ban đầu
    public float speedIncreasePerSecond = 0.1f; //Tăng tốc theo thời gian 0.1 giây
    public float currentScoreIncreaseSpeedMultiplayer = 2f; //Hệ số tăng điểm theo tốc độ

    //Quản lý điểm số game
    [Header("Điểm số của người chơi")]
    public TextMeshProUGUI scoreText; //Hiển thị điểm hiện tại + cao nhất người chơi 
    public TextMeshProUGUI diamondScore; //Hiển thị kim cương ở màn hình kết thúc
    public GameObject gameEndScreen; //Giao diện khi kết thúc game

    //Quản lý chướng ngại vật(trap) và kim cương(diamond)
    [Header("Thời gian để hiện chướng ngại vật trong game")]
    public float minTimeDelayBetweenTrap = 2f;
    public float maxTimeDelayBetweenTrap = 2f;
    public float trapSpeedMultiplier = 3f; //Hệ số tốc độ di chuyển của trap

    [Header("Những kiểu chướng ngại vật")]
    public GameObject[] allTreeInGround;
    public GameObject[] allStoneInGround;
    public GameObject[] allAnimalInGround;
    public GameObject[] allFlyingTrap;
    public GameObject[] allThickingTrap;
    public GameObject[] allReptileTrap;

    //Điểm spawn trap
    public Transform treeTrapSpamPoint;
    public Transform stoneTrapSpamPoint;
    public Transform animalTrapSpamPoint;
    public Transform flyingTrapSpamPoint;
    public Transform thickingTrapSpamPoint;
    public Transform reptileTrapSpamPoint;

    private List<GameObject> allCurrentTrap = new List<GameObject>();


    //Quản lý âm thanh
    [Header("SPX")]
    [SerializeField] public AudioSource audioSource;
    public AudioClip pointSPX; //Âm thanh khi đạt mốc điểm
    public AudioClip diamondSPX; //Âm thanh khi ăn diamond


    //Biển lưu điểm của người chơi
    [Header("UI Điểm số")]
    public TextMeshProUGUI finalScoreText;

    private float currentSpeed;
    private int diemSoCao = 0;
    private float currentDiemso = 0;

    //Biến chuyển cảnh ngày/đêm
    [Header("Global Light chuyển cảnh")]
    [SerializeField] private Light2D globalLight2D;
    private bool isNight = false;
    private int nextBackgroundChangeScene = 100; //Sau mỗi 100 điểm -> sẽ đổi từ ngày sang đêm và ngược lại

    //Biến kim cương(diamond)
    [Header("Kim cương")]
    public TextMeshProUGUI diamondText;
    private int diamondAmount = 0;
    public GameObject[] diamondPrefab;
    public Transform diamondSpampoint;
    private List<GameObject> allCurrentDiamonds = new List<GameObject>();

    public float diamondSpeedMultiplier = 1.5f; // ← Tốc độ riêng cho kim cương

    private enum SpawnType { Diamond, Trap } //Loại vật thể sẽ spawn tiếp theo
    private SpawnType nextSpawn = SpawnType.Diamond;

    private float spawnTimer = 2f; //Thời gian chờ giữa các lần spawn

    //Biến hiện kiểm tra FPS
    [Header("Check FPS(Frame Per Second)")]
    [SerializeField] private TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;

    //Xử lý kim cương(diamond)
    public void AddDiamond(int amount)
    {
        diamondAmount += amount; //Cộng thêm kim cương -> player chạm vào kim cương
        UpdateDiamondUI();

        //Phát âm thanh khi player ăn kim cương
        if(diamondSPX != null)
        {
            audioSource.PlayOneShot(diamondSPX);
        }
    }

    public void RemoveDiamond(GameObject diamond)
    {
        //Xóa kim cương khỏi scene game khi đã ăn từ plauer
        if (allCurrentDiamonds.Contains(diamond))
        {
            allCurrentDiamonds.Remove(diamond);
        }
    }

    private void UpdateDiamondUI()
    {
        diamondText.text = diamondAmount.ToString("D3"); //Hiển thị số lần ăn diamond dạng 3 chữ số(001)
    }

    //Hàm awake
    private void Awake()
    {
        Instance = this;
        currentSpeed = startingSpeed;

        //Lấy điểm cao nhất lưu từ màn chơi trước
        if (PlayerPrefs.HasKey("Điểm số cao"))
        {
            diemSoCao = PlayerPrefs.GetInt("Điểm số cao");
        }

        //Thiết lập FPS
        QualitySettings.vSyncCount = 0; 
        int savedFps = PlayerPrefs.GetInt("FPSSetting", 60);
        Application.targetFrameRate = savedFps;

        UpdateScore();
        Time.timeScale = 1;
        Debug.Log("FPS hiện tại: " + Application.targetFrameRate);
    }

    //Hàm hiện màn hình kết thúc game
    public void ShowEndScreen()
    {
        gameEndScreen.SetActive(true);
        finalScoreText.text = $"SCORE: {Mathf.RoundToInt(currentDiemso):D5}";
        diamondScore.text = $": {diamondAmount:D3}";
    }

    //Hàm Update
    private void Update()
    {
        if (batDauGame && !ketThucGame)
        {
            //Bộ đếm thời gian spawn
            spawnTimer -= Time.deltaTime;
            if(spawnTimer <= 0f)
            {
                if(nextSpawn == SpawnType.Diamond)
                {
                    SpawnDiamond();
                    nextSpawn = SpawnType.Trap;
                    spawnTimer = Random.Range(2.5f, 4f);
                    Debug.Log("Ăn diamond");
                }
                else if(nextSpawn == SpawnType.Trap)
                {
                    SpawnRandomTrap();
                    nextSpawn = SpawnType.Diamond;
                    spawnTimer = Random.Range(1.5f, 3f);
                    Debug.Log("Vượt trap");

                }
            }

            //Di chuyển tất cả chướng ngại vật sang bên trái
            foreach (GameObject trap in allCurrentTrap)
            {
                trap.transform.Translate(Vector3.left * currentSpeed * Time.deltaTime * trapSpeedMultiplier);
            }

            //Di chuyển kim cương
            for (int i = allCurrentDiamonds.Count - 1; i >= 0; i--)
            {
                if (allCurrentDiamonds[i] == null)
                {
                    allCurrentDiamonds.RemoveAt(i);
                }
                else
                {
                    allCurrentDiamonds[i].transform.Translate(Vector3.left * currentSpeed * Time.deltaTime * diamondSpeedMultiplier);
                }
            }

            //Tăng tốc độ game theo thời gian
            currentSpeed += Time.deltaTime * speedIncreasePerSecond;

            //Cuộn nền (scroll texture)
            groundMeshRenderer.material.mainTextureOffset += new Vector2(currentSpeed * Time.deltaTime, 0);
            backgroundMeshRenderer.material.mainTextureOffset += new Vector2(currentSpeed * Time.deltaTime, 0);

            //Cập nhật điểm số
            int lancuoi = Mathf.RoundToInt(currentDiemso);
            currentDiemso += currentSpeed * Time.deltaTime * currentScoreIncreaseSpeedMultiplayer;
            int scoreInt = Mathf.RoundToInt(currentDiemso);

            //Sau mỗi lần player đạt 100 điểm -> đổi cảnh sang sáng/tối
            if (scoreInt >= nextBackgroundChangeScene)
            {
                StartCoroutine(SwitchLighting());
                nextBackgroundChangeScene += 100;
            }

            //Phát ẩm thanh mỗi khi đạt mốc 1000 điểm
            if (scoreInt > lancuoi && scoreInt % 1000 == 0)
            {
                audioSource.clip = pointSPX;
                audioSource.Play();
            }

            //Lưu điểm cao nhất
            if (scoreInt > diemSoCao)
            {
                diemSoCao = scoreInt;
                PlayerPrefs.SetInt("Điểm số cao", diemSoCao);
            }  

            UpdateScore();
        }

        //Tính và hiển thị FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if(fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.CeilToInt(fps)}";
        }
    }

    //Sinh ngẫu nhiên TRAP
    void SpawnRandomTrap()
    {
        GameObject newTrap = null;
        if (currentDiemso >= 25)
        {
            float rand = Random.value;
            if (rand < 0.2f)
                newTrap = Instantiate(allFlyingTrap[Random.Range(0, allFlyingTrap.Length)], flyingTrapSpamPoint.position, Quaternion.identity);
            else if (rand < 0.4f)
                newTrap = Instantiate(allStoneInGround[Random.Range(0, allStoneInGround.Length)], stoneTrapSpamPoint.position, Quaternion.identity);
            else if (rand < 0.6f)
                newTrap = Instantiate(allAnimalInGround[Random.Range(0, allAnimalInGround.Length)], animalTrapSpamPoint.position, Quaternion.identity);
            else if (rand < 0.75f)
                newTrap = Instantiate(allTreeInGround[Random.Range(0, allTreeInGround.Length)], treeTrapSpamPoint.position, Quaternion.identity);
            else if (rand < 0.9f)
                newTrap = Instantiate(allThickingTrap[Random.Range(0, allThickingTrap.Length)], thickingTrapSpamPoint.position, Quaternion.identity);
            else
                newTrap = Instantiate(allReptileTrap[Random.Range(0, allReptileTrap.Length)], reptileTrapSpamPoint.position, Quaternion.identity);
        }
        else
        {
            //Lúc đầu chỉ xuất hiện cây(đơn giản)
            newTrap = Instantiate(allTreeInGround[Random.Range(0, allTreeInGround.Length)], treeTrapSpamPoint.position, Quaternion.identity);
        }

        allCurrentTrap.Add(newTrap);
    }

    //Hàm hiện kim cương
    void SpawnDiamond()
    {
        if (Random.value > 0.1f) //Xác suất xuất hiện 90%
        {
            GameObject diamond = Instantiate(diamondPrefab[0], diamondSpampoint.position, Quaternion.identity);
            allCurrentDiamonds.Add(diamond);
        }
    }

    //Hàm chuyển đổi ngày và đêm 
    private IEnumerator SwitchLighting() 
    {
        isNight = !isNight;
        Color dayColor = Color.white;
        Color nightColor = new Color(0.2f, 0.2f, 0.4f, 1f);
        Color from = globalLight2D.color;
        Color to = isNight ? nightColor : dayColor;

        float time = 0;
        float duration = 2f;
        while (time < duration)
        {
            time += Time.deltaTime;
            globalLight2D.color = Color.Lerp(from, to, time / duration);
            yield return null;
        }
    }

    //Hàm khởi tạo ánh sáng ban đầu
    private void Start()
    {
        // ánh sáng ban đầu dựa theo setting đã chọn ở menu
        isNight = !gameData.startWithDay;

        if (isNight)
            globalLight2D.color = new Color(0.2f, 0.2f, 0.4f, 1f);// đêm
        else
            globalLight2D.color = Color.white; //Ban ngày
    }

    //Hàm cập nhật giao diện điểm
    private void UpdateScore()
    {
        scoreText.SetText($"HI {diemSoCao:D5}  {Mathf.RoundToInt(currentDiemso):D5}");
    }

    //Hàm chuyển Scene -> khi mà người chơi bấm nút restart
    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void MainMenuGame()
    {
        SceneManager.LoadScene("LoadingScene");
        LoadingScene.sceneToLoad = "MainMenu";
        Time.timeScale = 1;
    }
}
