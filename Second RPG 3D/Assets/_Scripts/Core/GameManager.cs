// GameManager.cs (Phiên bản đã thêm chức năng quản lý chuột)
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

// Đặt enum GameState ra ngoài để các script khác dễ truy cập
public enum GameState
{
    GAMEPLAY,
    WIN,
    DIE,
    PAUSE
}

public class GameManager : MonoBehaviour
{
    // --- Singleton: Đảm bảo chỉ có một GameManager và dễ dàng truy cập ---
    public static GameManager Instance { get; private set; }

    // --- Trạng thái game ---
    public GameState gameState;

    [Header("Core References")]
    public Transform player;

    [Header("UI Panels & Texts")]
    public GameObject pauseGame;
    public GameObject panelDie;
    public TMP_Text scoreDie;
    public TMP_Text tx; // Giả sử đây là Text Score
    public GameObject panelWin;
    public GameObject panelHuongDan;
    [Header("Monsters UI")]
    public TMP_Text soLuong;

    // --- Quản lý Kẻ địch (Tự động) ---
    private List<BaseAIController> activeEnemies = new List<BaseAIController>();

    [Header("Gem")]
    public GameObject gemObject;

    [Header("Rain Manager")]
    public PostProcessVolume postB;
    public ParticleSystem rainParticle;
    private ParticleSystem.EmissionModule rainModule;
    public int rainRateOverTime;
    public int rainIncrement;
    public float rainIncrementDelay;

    [Header("Camera")]
    public GameObject cam1;
    public GameObject cam2;
    public bool checkCam;

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Tự động tìm Player bằng Tag nếu chưa được gán trong Inspector
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else Debug.LogError("GAME MANAGER ERROR: Không tìm thấy đối tượng nào có tag 'Player' trong scene!");
        }
    }

    void Start()
    {
        if (rainParticle != null) rainModule = rainParticle.emission;

        // Tự động tìm và đếm tất cả kẻ địch trong màn chơi
        BaseAIController[] allEnemies = FindObjectsOfType<BaseAIController>();
        activeEnemies.AddRange(allEnemies);
        if (soLuong != null) soLuong.text = activeEnemies.Count.ToString();
        Debug.Log("GameManager: Started with " + activeEnemies.Count + " enemies.");

        // Ẩn tất cả các panel không cần thiết
        if (panelDie != null) panelDie.SetActive(false);
        if (panelWin != null) panelWin.SetActive(false);
        if (pauseGame != null) pauseGame.SetActive(false);
        if (panelHuongDan != null) panelHuongDan.SetActive(false);

        // Bắt đầu game
        ChangeGameState(GameState.GAMEPLAY);
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

    // --- HÀM TỰ ĐỘNG GỌI BỞI KẺ ĐỊCH ---
    public void OnEnemyDefeated(BaseAIController defeatedEnemy)
    {
        if (activeEnemies.Contains(defeatedEnemy))
        {
            activeEnemies.Remove(defeatedEnemy);
            if (soLuong != null) soLuong.text = activeEnemies.Count.ToString();
            Debug.Log("GameManager: An enemy was defeated. Remaining: " + activeEnemies.Count);
        }

        // Tự động kiểm tra điều kiện thắng
        if (activeEnemies.Count == 0)
        {
            ChangeGameState(GameState.WIN);
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

    public void CreateGem(Transform pos)
    {
        Vector3 hi = pos.position;
        hi.y += 0.5f;
        Instantiate(gemObject, hi, Quaternion.Euler(90f, 0f, 0f));
    }

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

    IEnumerator RainManager(bool isRain)
    {
        // Đảm bảo rainModule đã được khởi tạo
        if (rainParticle == null) yield break;

        switch (isRain)
        {
            case true:
                // Tăng dần hiệu ứng mưa
                for (float i = rainModule.rateOverTime.constant; i < rainRateOverTime; i += rainIncrement)
                {
                    rainModule.rateOverTime = i;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = rainRateOverTime;
                break;
            case false:
                // Giảm dần hiệu ứng mưa
                for (float i = rainModule.rateOverTime.constant; i > 0; i -= rainIncrement)
                {
                    rainModule.rateOverTime = i;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = 0;
                break;
        }
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