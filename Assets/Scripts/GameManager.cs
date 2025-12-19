using UnityEngine;
using TMPro; // <- penting untuk TMP

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup startText;
    public CanvasGroup gameOverPanel;
    public TMP_Text countdownText;

    [Header("Spawn Settings")]
    public static bool isGameStarted = false;

    public GameObject playerPrefab;
    private GameObject spawnedPlayer;

    public CameraFollow cameraFollow;

    void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("SteveFullBody");
        if (playerPrefab == null)
            Debug.LogError("Player.prefab tidak ditemukan di Resources!");

        if (cameraFollow == null)
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

        // set camera awal
        Vector3 camStartPos = new Vector3(0f, 10f, -10f);
        Camera.main.transform.position = camStartPos;

        // UI awal
        ShowStartText(true);
        ShowGameOver(false);
    }

    void Update()
    {
        if (!isGameStarted && Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        isGameStarted = true;

        // hilangkan teks start
        ShowStartText(false);

        // spawn player
        Vector3 spawnPos = new Vector3(0f, 10f, 0f);
        spawnedPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        cameraFollow.target = spawnedPlayer.transform;
    }

    public void PlayerDied()
    {
        if (!isGameStarted) return;

        isGameStarted = false; // freeze semua script

        ShowGameOver(true);

        StartCoroutine(GameOverCountdown());
    }

    private System.Collections.IEnumerator GameOverCountdown()
    {
        int count = 3;

        while (count > 0)
        {
            countdownText.text = "Restarting in " + count + "...";
            yield return new WaitForSeconds(1f);
            count--;
        }

        RestartGame();
    }

    public void RestartGame()
    {
        // hapus player
        if (spawnedPlayer != null)
            Destroy(spawnedPlayer);

        // reset kamera
        Vector3 camStartPos = new Vector3(0f, 10f, -10f);
        Camera.main.transform.position = camStartPos;

        // UI transisi
        ShowGameOver(false);
        ShowStartText(true);

        Debug.Log("Game restarted. Tekan Enter untuk mulai.");
    }

    // UI functions
    void ShowStartText(bool show)
    {
        if (startText == null) return;
        startText.alpha = show ? 1 : 0;
        startText.interactable = show;
        startText.blocksRaycasts = show;
    }

    void ShowGameOver(bool show)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.alpha = show ? 1 : 0;
        gameOverPanel.interactable = show;
        gameOverPanel.blocksRaycasts = show;
    }
}
