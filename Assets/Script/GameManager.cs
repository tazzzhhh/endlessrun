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
    [SerializeField] private MeshRenderer groundMeshRenderer;
    [SerializeField] private MeshRenderer backgroundMeshRenderer;

    public static GameManager Instance;

    [HideInInspector] public bool batDauGame = false;
    [HideInInspector] public bool ketThucGame = false;

    [Header("Speeding của game")]
    public float startingSpeed = 5f;
    public float speedIncreasePerSecond = 0.1f;
    public float currentScoreIncreaseSpeedMultiplayer = 2f;

    [Header("Điểm số của người chơi")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI diamondScore;
    public GameObject gameEndScreen;

    [Header("Thời gian để hiện chướng ngại vật trong game")]
    public float minTimeDelayBetweenTrap = 2f;
    public float maxTimeDelayBetweenTrap = 2f;
    public float trapSpeedMultiplier = 3f;

    [Header("Những kiểu chướng ngại vật")]
    public GameObject[] allTreeInGround;
    public GameObject[] allStoneInGround;
    public GameObject[] allAnimalInGround;
    public GameObject[] allFlyingTrap;
    public GameObject[] allThickingTrap;
    public GameObject[] allReptileTrap;

    public Transform treeTrapSpamPoint;
    public Transform stoneTrapSpamPoint;
    public Transform animalTrapSpamPoint;
    public Transform flyingTrapSpamPoint;
    public Transform thickingTrapSpamPoint;
    public Transform reptileTrapSpamPoint;

    private List<GameObject> allCurrentTrap = new List<GameObject>();

    [Header("SPX")]
    [SerializeField] public AudioSource audioSource;
    public AudioClip pointSPX;
    public AudioClip diamondSPX; //Âm thanh khi ăn diamond

    [Header("UI Điểm số")]
    public TextMeshProUGUI finalScoreText;

    private float currentSpeed;
    private int diemSoCao = 0;
    private float currentDiemso = 0;

    [Header("Global Light chuyển cảnh")]
    [SerializeField] private Light2D globalLight2D;
    private bool isNight = false;
    private int nextBackgroundChangeScene = 100;

    [Header("Kim cương")]
    public TextMeshProUGUI diamondText;
    private int diamondAmount = 0;
    public GameObject[] diamondPrefab;
    public Transform diamondSpampoint;
    private List<GameObject> allCurrentDiamonds = new List<GameObject>();

    public float diamondSpeedMultiplier = 1.5f; // ← Tốc độ riêng cho kim cương

    private enum SpawnType { Diamond, Trap }
    private SpawnType nextSpawn = SpawnType.Diamond;

    private float spawnTimer = 2f;

    [Header("Check FPS(Frame Per Second)")]
    [SerializeField] private TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;

    public void AddDiamond(int amount)
    {
        diamondAmount += amount;
        UpdateDiamondUI();

        if(diamondSPX != null)
        {
            audioSource.PlayOneShot(diamondSPX);
        }
    }

    public void RemoveDiamond(GameObject diamond)
    {
        if (allCurrentDiamonds.Contains(diamond))
        {
            allCurrentDiamonds.Remove(diamond);
        }
    }

    private void UpdateDiamondUI()
    {
        diamondText.text = diamondAmount.ToString("D3");
    }


    private void Awake()
    {
        Instance = this;
        currentSpeed = startingSpeed;

        if (PlayerPrefs.HasKey("Điểm số cao"))
        {
            diemSoCao = PlayerPrefs.GetInt("Điểm số cao");
        }
        QualitySettings.vSyncCount = 0; 
        int savedFps = PlayerPrefs.GetInt("FPSSetting", 60);
        Application.targetFrameRate = savedFps;

        UpdateScore();
        Time.timeScale = 1;
        Debug.Log("FPS hiện tại: " + Application.targetFrameRate);
    }

    public void ShowEndScreen()
    {
        gameEndScreen.SetActive(true);
        finalScoreText.text = $"SCORE: {Mathf.RoundToInt(currentDiemso):D5}";
        diamondScore.text = $": {diamondAmount:D3}";
    }

    private void Update()
    {
        if (batDauGame && !ketThucGame)
        {
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

            foreach (GameObject trap in allCurrentTrap)
            {
                trap.transform.Translate(Vector3.left * currentSpeed * Time.deltaTime * trapSpeedMultiplier);
            }

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


            currentSpeed += Time.deltaTime * speedIncreasePerSecond;
            groundMeshRenderer.material.mainTextureOffset += new Vector2(currentSpeed * Time.deltaTime, 0);
            backgroundMeshRenderer.material.mainTextureOffset += new Vector2(currentSpeed * Time.deltaTime, 0);

            int lancuoi = Mathf.RoundToInt(currentDiemso);
            currentDiemso += currentSpeed * Time.deltaTime * currentScoreIncreaseSpeedMultiplayer;

            int scoreInt = Mathf.RoundToInt(currentDiemso);
            if (scoreInt >= nextBackgroundChangeScene)
            {
                StartCoroutine(SwitchLighting());
                nextBackgroundChangeScene += 100;
            }

            if (scoreInt > lancuoi && scoreInt % 1000 == 0)
            {
                audioSource.clip = pointSPX;
                audioSource.Play();
            }

            if (scoreInt > diemSoCao)
            {
                diemSoCao = scoreInt;
                PlayerPrefs.SetInt("Điểm số cao", diemSoCao);
            }  

            UpdateScore();
        }

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if(fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.CeilToInt(fps)}";
        }
    }

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
            newTrap = Instantiate(allTreeInGround[Random.Range(0, allTreeInGround.Length)], treeTrapSpamPoint.position, Quaternion.identity);
        }

        allCurrentTrap.Add(newTrap);
    }

    void SpawnDiamond()
    {
        if (Random.value > 0.1f)
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

    private void Start()
    {
        // ánh sáng ban đầu dựa theo setting đã chọn ở menu
        isNight = !gameData.startWithDay;

        if (isNight)
            globalLight2D.color = new Color(0.2f, 0.2f, 0.4f, 1f);// đêm
        else
            globalLight2D.color = Color.white;
    }

    private void UpdateScore()
    {
        scoreText.SetText($"HI {diemSoCao:D5}  {Mathf.RoundToInt(currentDiemso):D5}");
    }

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
