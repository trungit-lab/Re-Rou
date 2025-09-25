using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public enum enemyState
{
    IDLE, ALERT, PATROL, FOLLOW, FURY, EXPLORE, DIE
}
public enum GameState
{
    GAMEPLAY, DIE
}
public class GameManager : MonoBehaviour
{
    public GameState gameState;
    [Header("Slime AI")]
    //public Transform[] slimeWayPoints;
    public float slimeIdleWaitTime;
    public Transform player;
    public float slimeStopDistance;
    public float distanceToAttack;
    public float slimeAlertWaitTime;
    public float slimeAttackDelay;
    public float slimeLookAtSpeed;

    public TMP_Text tx;
    [Header("Camera")]
    public GameObject cam1;
    public GameObject cam2;
    public bool checkCam;

    [Header("Rain Manager")]
    public PostProcessVolume postB;
    public ParticleSystem rainParticle;
    private ParticleSystem.EmissionModule rainModule;
    public int rainRateOverTime;
    public int rainIncrement;
    public float rainIncrementDelay;

    [Header("Gem")]
    public GameObject gemObject;
    public float timeSpamGem;

    public GameObject pauseGame;
    public GameObject panelDie;
    public TMP_Text scoreDie;

    [Header("Monters")]
    public TMP_Text soLuong;

    public GameObject panelWin;
    public GameObject panelHuongDan;
    // Start is called before the first frame update
    void Start()
    {
        rainModule = rainParticle.emission;
    }

    // Update is called once per frame
    void Update()
    {
        //SpamGem(99.8f);
    }
    public void Plus()
    {
        int tam = int.Parse(tx.text);
        ++tam;
        tx.text = tam.ToString();
    }
    public void OnOffRain(bool isRain)
    {
        StopCoroutine("RainManager");
        StopCoroutine("PostBManager");
        StartCoroutine("RainManager", isRain);
        StartCoroutine("PostBManager", isRain);
    }
    IEnumerator RainManager(bool isRain)
    {
        switch (isRain)
        {
            case true:
                for (float i = rainModule.rateOverTime.constant; i < rainRateOverTime; i += rainIncrement)
                {
                    rainModule.rateOverTime = i;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }
                rainModule.rateOverTime = rainRateOverTime;
                break;
            case false:
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
        switch (isRain)
        {
            case true:
                for (float i = postB.weight; i < 1; i += Time.deltaTime)
                {
                    postB.weight = i;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 1;
                break;
            case false:
                for (float i = postB.weight; i > 0; i -= Time.deltaTime)
                {
                    postB.weight = i;
                    yield return new WaitForEndOfFrame();
                }
                postB.weight = 0;
                break;
        }
    }
    public void ChangeGameState(GameState newState)
    {
        gameState = newState;
    }
    public void SpamGem(float check)
    {
        if (Rand() > check)
        {
            Invoke("CreateGem", timeSpamGem);
        }
    }
    public void CreateGem(Transform pos)
    {
        Vector3 hi = pos.position;
        hi.y += 0.5f;
        Instantiate(gemObject, hi, Quaternion.Euler(90f, 0f, 0f), gameObject.transform);
    }
    public int Rand()
    {
        return Random.Range(0, 101);
    }
    public void ButtomPlay()
    {
        SceneManager.LoadScene("Gameplay");
    }
    public void ButtomExit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
    public void ButtomRank()
    {

    }
    public void ButtomHowToPlay()
    {
        panelHuongDan.SetActive(true);
    }
    public void ButtomHome()
    {
        SceneManager.LoadScene("UI");
        Time.timeScale = 1f;
    }
    public void ButtomPause()
    {
        Time.timeScale = 0f;
        pauseGame.SetActive(true);
    }
    public void ButtomResume()
    {
        Time.timeScale = 1f;
        pauseGame.SetActive(false);
    }
    public void ButtomRePlay()
    {
        SceneManager.LoadScene("Gameplay");
    }
    public void DieDone()
    {
        panelDie.SetActive(true);
        scoreDie.text = "Score: " + tx.text;
    }
    public void GiamSoLuong()
    {
        int tam = int.Parse(soLuong.text);
        --tam;
        soLuong.text = tam.ToString();
        if (tam == 0)
        {
            Win();
        }
    }
    public void Win()
    {
        Time.timeScale = 0f;
        panelWin.SetActive(true);
    }
}
