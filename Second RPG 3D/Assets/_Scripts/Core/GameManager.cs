// FILE: GameManager.cs (Phiên bản nâng cấp cho Wave Spawner)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public enum GameState
{
    GAMEPLAY,
    WIN,
    DIE,
    PAUSE
}

public class GameManager : MonoBehaviour
{
    // --- Singleton: Giữ nguyên ---
    public static GameManager Instance { get; private set; }

    // Trong GameManager.cs, thêm vào khu vực UI Panels & Texts
    [Header("Wave UI")]
    public TMP_Text waveCountdownText; // Kéo Text đếm ngược vào đây
    public TMP_Text currentWaveText;   // (Tùy chọn) Text hiển thị "Sóng 1/3"

    // --- Trạng thái game: Giữ nguyên ---
    public GameState gameState;

    [Header("Core References")]
    public Transform player;
    // --- THÊM MỚI: Kết nối đến Wave Spawner ---
    [Tooltip("Kéo đối tượng WaveSpawner trong scene vào đây.")]
    public WaveSpawner waveSpawner;

    [Header("UI Panels & Texts")]
    public GameObject pauseGame;
    public GameObject panelDie;
    public TMP_Text scoreDie;
    public TMP_Text tx;
    public GameObject panelWin;
    public GameObject panelHuongDan;
    [Header("Monsters UI")]
    public TMP_Text soLuong; // Sẽ hiển thị số quái còn lại trong sóng

    // --- THAY ĐỔI: Danh sách này không còn cần thiết nữa ---
    // GameManager không cần tự quản lý danh sách này, nó chỉ cần biết "số lượng".
    // private List<BaseAIController> activeEnemies = new List<BaseAIController>();
    private int enemiesRemaining; // Biến mới để theo dõi số lượng

    // --- Các biến khác giữ nguyên ---
    [Header("Gem")]
    public TMP_Text diem;
    private int score_d;
    [Header("Rain Manager")]
    public PostProcessVolume postB;



    // --- Hàm Awake: Giữ nguyên ---
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    // --- HÀM START: ĐƯỢC ĐƠN GIẢN HÓA RẤT NHIỀU ---
    void Start()
    {
        // if (rainParticle != null) rainModule = rainParticle.emission; // Sẽ gây lỗi nếu bạn không có các biến rain cũ, tôi tạm comment lại

        // --- THAY ĐỔI: Xóa bỏ hoàn toàn logic đếm quái cũ ---
        // Không cần tự đi tìm quái nữa, WaveSpawner sẽ thông báo cho chúng ta.
        enemiesRemaining = 0;
        if (soLuong != null) soLuong.text = enemiesRemaining.ToString();

        // Ẩn tất cả các panel không cần thiết (giữ nguyên)
        if (panelDie != null) panelDie.SetActive(false);
        if (panelWin != null) panelWin.SetActive(false);
        if (pauseGame != null) pauseGame.SetActive(false);
        if (panelHuongDan != null) panelHuongDan.SetActive(false);

        // Bắt đầu game (giữ nguyên)
        ChangeGameState(GameState.GAMEPLAY);
    }


    public void UpdateWaveUI(int currentWave, int totalWaves, float countdown)
    {
        if (currentWaveText != null)
        {
            currentWaveText.text = $"{currentWave}";
        }

        if (waveCountdownText != null)
        {
            // Định dạng thời gian thành phút:giây
            int minutes = Mathf.FloorToInt(countdown / 60);
            int seconds = Mathf.FloorToInt(countdown % 60);
            waveCountdownText.text = $"{minutes:00}:{seconds:00}";
        }
    }


    // === THÊM MỚI: Hàm quản lý con trỏ chuột ===
    private void SetCursorState(bool locked)
    {
        if (locked)
        {
            // Ẩn và khóa chuột vào giữa màn hình
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Hiện và mở khóa chuột
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- HÀM QUẢN LÝ TRẠNG THÁI TRUNG TÂM ---
    public void ChangeGameState(GameState newState)
    {
        if (gameState == newState && newState != GameState.GAMEPLAY) return;

        gameState = newState;
        Debug.Log("GameManager: State changed to " + newState.ToString());

        switch (gameState)
        {
            case GameState.GAMEPLAY:
                Time.timeScale = 1f;
                SetCursorState(true); // THÊM VÀO: Ẩn chuột khi chơi
                break;

            case GameState.WIN:
                Time.timeScale = 0f;
                if (panelWin != null) panelWin.SetActive(true);
                SetCursorState(false); // THÊM VÀO: Hiện chuột khi thắng
                break;

            case GameState.DIE:
                Time.timeScale = 0f;
                if (panelDie != null)
                {
                    panelDie.SetActive(true);
                    if (scoreDie != null && tx != null) scoreDie.text = "Score: " + tx.text;
                }
                SetCursorState(false); // THÊM VÀO: Hiện chuột khi thua
                break;

            case GameState.PAUSE:
                Time.timeScale = 0f;
                if (pauseGame != null) pauseGame.SetActive(true);
                SetCursorState(false); // THÊM VÀO: Hiện chuột khi tạm dừng
                break;
        }
    }


    // --- HÀM THÊM MỚI: Được WaveSpawner gọi khi một sóng mới bắt đầu ---
    public void OnWaveStarted(int enemyCountInWave)
    {
        enemiesRemaining = enemyCountInWave;
        Debug.Log("GameManager: Một sóng mới bắt đầu với " + enemyCountInWave + " kẻ địch.");
        if (soLuong != null)
        {
            soLuong.text = enemyCountInWave.ToString();
        }

    }

    // --- HÀM OnEnemyDefeated: ĐƯỢC CẬP NHẬT LOGIC ---
    public void OnEnemyDefeated(BaseAIController defeatedEnemy)
    {
        enemiesRemaining--;
        
        if (soLuong != null)
        {
            soLuong.text = enemiesRemaining.ToString();
        }
        Debug.Log("GameManager: Một kẻ địch bị tiêu diệt. Còn lại: " + enemiesRemaining);

        // THAY ĐỔI: Không cần tự kiểm tra điều kiện thắng ở đây nữa.
        // Thay vào đó, báo cho WaveSpawner biết để nó quyết định.
        if (waveSpawner != null)
        {
            waveSpawner.OnAnEnemyWasKilled();

        }
    }


    // --- CÁC HÀM TIỆN ÍCH KHÁC ---

    public void DieDone()
    {
        // Hàm này bây giờ sẽ là một "lối tắt" để gọi đến hệ thống quản lý trạng thái chính
        ChangeGameState(GameState.DIE);
    }
    public void Plus()
    {
        int tam = int.Parse(tx.text);
        ++tam;
        tx.text = tam.ToString();
    }

    //public void CreateGem(Transform pos)
    //{
    //    Vector3 hi = pos.position;
    //    hi.y += 0.5f;
    //    Instantiate(gemObject, hi, Quaternion.Euler(90f, 0f, 0f));
    //}

    // --- CÁC HÀM ĐIỀU KHIỂN NÚT BẤM (ĐÃ CẢI TIẾN) ---

    public void ButtomPause()
    {
        ChangeGameState(GameState.PAUSE);
    }

    public void ButtomResume()
    {
        ChangeGameState(GameState.GAMEPLAY);
        if (pauseGame != null) pauseGame.SetActive(false);
    }

    public void ButtomRePlay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ButtomHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UI"); // Giả sử scene menu của bạn tên là "UI"
    }

    public void ButtomHowToPlay()
    {
        if (panelHuongDan != null) panelHuongDan.SetActive(true);
    }

    public void ButtomExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnOffRain(bool isRain)
    {
        // Dừng các coroutine cũ trước khi bắt đầu cái mới để tránh chạy song song
        StopCoroutine("RainManager");
        StopCoroutine("PostBManager");
        StartCoroutine("RainManager", isRain);
        StartCoroutine("PostBManager", isRain);
    }



    IEnumerator PostBManager(bool isRain)
    {
        // Đảm bảo PostProcessVolume đã được gán
        if (postB == null) yield break;

        switch (isRain)
        {
            case true:
                // Tăng dần hiệu ứng xử lý hậu kỳ
                for (float i = postB.weight; i < 1; i += Time.deltaTime)
                {
                    postB.weight = i;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 1;
                break;
            case false:
                // Giảm dần hiệu ứng xử lý hậu kỳ
                for (float i = postB.weight; i > 0; i -= Time.deltaTime)
                {
                    postB.weight = i;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 0;
                break;
        }
    }
}