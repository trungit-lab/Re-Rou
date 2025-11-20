using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

// Enum trạng thái game
public enum GameState
{
    GAMEPLAY,
    WIN,
    DIE,
    PAUSE
}

public class GameManager : MonoBehaviour
{
    #region 1. SINGLETON PATTERN
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Tự động tìm Player nếu chưa gán
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }
    #endregion

    #region 2. INSPECTOR VARIABLES

    [Header("--- GAME STATE & CORE ---")]
    public GameState gameState;
    public Transform player;
    [Tooltip("Kéo WaveSpawner vào đây")]
    public WaveSpawner waveSpawner;

    [Header("--- WAVE UI ---")]
    public TMP_Text waveCountdownText;
    public TMP_Text currentWaveText;

    [Header("--- UI PANELS ---")]
    public GameObject pauseGame;
    public GameObject panelDie;
    public GameObject panelWin;
    //public GameObject panelHuongDan;

    [Header("--- UI TEXTS & SCORE ---")]
    public TMP_Text scoreDie;
    public TMP_Text tx;      // Text hiển thị điểm số hiện tại
    public TMP_Text soLuong; // Hiển thị số lượng quái còn lại
    public TMP_Text diem;    // Gem score (nếu có)

    [Header("--- VISUALS (RAIN & POST PROCESS) ---")]
    public PostProcessVolume postB;

    #endregion

    #region 3. PRIVATE VARIABLES
    private int enemiesRemaining = 0;
    private Coroutine hitStopCoroutine;
    private Coroutine rainEffectCoroutine;
    private Coroutine postProcessCoroutine;
    #endregion

    #region 4. UNITY LIFECYCLE

    private void Start()
    {
        // Reset trạng thái ban đầu
        enemiesRemaining = 0;
        UpdateEnemyCountUI();

        // Đảm bảo tắt hết các UI không cần thiết
        panelDie?.SetActive(false);
        panelWin?.SetActive(false);
        pauseGame?.SetActive(false);
        //panelHuongDan?.SetActive(false);
        waveCountdownText?.gameObject.SetActive(false);

        // Bắt đầu game
        ChangeGameState(GameState.GAMEPLAY);
    }

    private void Update()
    {
        // CHỨC NĂNG QUAN TRỌNG: Bấm ESC để Pause/Resume
        // Vì khi ẩn chuột, người chơi không thể bấm nút Pause trên màn hình được.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameState == GameState.GAMEPLAY)
            {
                ButtomPause(); // Gọi hàm Pause
            }
            else if (gameState == GameState.PAUSE)
            {
                ButtomResume(); // Gọi hàm Resume
            }
            //// Nếu đang ở panel Hướng dẫn thì tắt nó đi
            //else if (panelHuongDan != null && panelHuongDan.activeSelf)
            //{
            //    panelHuongDan.SetActive(false);
            //}
        }
    }

    #endregion

    #region 5. STATE MANAGEMENT (QUẢN LÝ TRẠNG THÁI)

    public void ChangeGameState(GameState newState)
    {
        gameState = newState;
        Debug.Log($"GameManager: Changed State to {newState}");

        switch (gameState)
        {
            case GameState.GAMEPLAY:
                Time.timeScale = 1f;
                SetCursorState(false); // Ẩn chuột để chơi game
                break;

            case GameState.PAUSE:
                Time.timeScale = 0f;
                pauseGame?.SetActive(true);
                SetCursorState(true); // Hiện chuột để bấm menu
                break;

            case GameState.WIN:
                Time.timeScale = 0f; // Hoặc để 1f nếu muốn slow motion
                panelWin?.SetActive(true);
                SetCursorState(true);
                break;

            case GameState.DIE:
                Time.timeScale = 0f;
                HandleDeathUI();
                SetCursorState(true);
                break;
        }
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleDeathUI()
    {
        if (panelDie != null)
        {
            panelDie.SetActive(true);
            // Copy điểm số từ màn hình chơi sang màn hình chết
            if (scoreDie != null && tx != null)
            {
                scoreDie.text = "Score: " + tx.text;
            }
        }
    }

    #endregion

    #region 6. WAVE SYSTEM LOGIC

    // Được gọi từ WaveSpawner
    public void OnWaveStarted(int enemyCount)
    {
        enemiesRemaining = enemyCount;
        UpdateEnemyCountUI();
        // Có thể thêm hiệu ứng âm thanh bắt đầu wave ở đây
    }

    // Được gọi khi quái chết (từ code AI hoặc Health)
    public void OnEnemyDefeated(BaseAIController defeatedEnemy = null)
    {
        enemiesRemaining--;
        if (enemiesRemaining < 0) enemiesRemaining = 0;

        UpdateEnemyCountUI();

        // Báo ngược lại cho Spawner biết (để tính toán Win/Next Wave)
        waveSpawner?.OnAnEnemyWasKilled();
    }

    private void UpdateEnemyCountUI()
    {
        if (soLuong != null)
        {
            soLuong.text = enemiesRemaining.ToString();
        }
    }

    public void UpdateWaveUI(string message, float countdown = -1f)
    {
        if (currentWaveText != null) currentWaveText.text = message;

        if (waveCountdownText != null)
        {
            if (countdown >= 0)
            {
                waveCountdownText.gameObject.SetActive(true);
                // Chuyển đổi float sang phút:giây cho đẹp
                int minutes = Mathf.FloorToInt(countdown / 60);
                int seconds = Mathf.FloorToInt(countdown % 60);
                waveCountdownText.text = $"Wave Start In: {minutes:00}:{seconds:00}";
            }
            else
            {
                waveCountdownText.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region 7. UI BUTTON EVENTS (GIỮ NGUYÊN TÊN ĐỂ KHÔNG LỖI EDITOR)

    // Nút Pause (hoặc gọi từ phím ESC)
    public void ButtomPause()
    {
        if (gameState == GameState.DIE || gameState == GameState.WIN) return;
        ChangeGameState(GameState.PAUSE);
    }

    // Nút Resume
    public void ButtomResume()
    {
        if (pauseGame != null) pauseGame.SetActive(false);
        ChangeGameState(GameState.GAMEPLAY);
    }

    // Nút Replay
    public void ButtomRePlay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Nút Home
    public void ButtomHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UI"); // Đảm bảo scene tên là "UI"
    }

    // Nút Hướng dẫn
    //public void ButtomHowToPlay()
    //{
    //    panelHuongDan?.SetActive(true);
    //}

    // Nút Thoát
    public void ButtomExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // Logic cộng điểm
    public void Plus()
    {
        if (tx != null && int.TryParse(tx.text, out int currentScore))
        {
            currentScore++;
            tx.text = currentScore.ToString();
        }
    }

    // Hàm gọi khi Player chết (để các script khác gọi ngắn gọn)
    public void DieDone()
    {
        ChangeGameState(GameState.DIE);
    }

    #endregion

    #region 8. VISUAL EFFECTS (HIT STOP & RAIN)

    // Hiệu ứng khựng hình khi đánh trúng
    public void TriggerHitStop(float duration)
    {
        if (gameState != GameState.GAMEPLAY) return;

        if (hitStopCoroutine != null) StopCoroutine(hitStopCoroutine);
        hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float originalScale = Time.timeScale;
        Time.timeScale = 0.05f; // Slow motion cực chậm
        yield return new WaitForSecondsRealtime(duration);

        // Chỉ trả lại TimeScale nếu game vẫn đang chơi (không bị pause giữa chừng)
        if (gameState == GameState.GAMEPLAY)
        {
            Time.timeScale = 1f;
        }
        hitStopCoroutine = null;
    }

    // Quản lý hiệu ứng mưa và Post Processing
    public void OnOffRain(bool isRain)
    {
        if (postProcessCoroutine != null) StopCoroutine(postProcessCoroutine);
        postProcessCoroutine = StartCoroutine(PostProcessFade(isRain));

        // Nếu có logic Particle Mưa, thêm vào đây
        // if (isRain) rainSystem.Play(); else rainSystem.Stop();
    }

    private IEnumerator PostProcessFade(bool enableEffect)
    {
        if (postB == null) yield break;

        float targetWeight = enableEffect ? 1f : 0f;
        float startWeight = postB.weight;
        float duration = 1.0f; // Thời gian chuyển đổi
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // Dùng deltaTime bình thường vì hiệu ứng này chạy ở Gameplay
            postB.weight = Mathf.Lerp(startWeight, targetWeight, elapsed / duration);
            yield return null; // Chờ frame tiếp theo
        }
        postB.weight = targetWeight;
    }

    #endregion
}