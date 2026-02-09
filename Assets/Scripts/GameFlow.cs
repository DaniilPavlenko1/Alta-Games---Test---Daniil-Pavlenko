using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Win, Lose }

public class GameFlow : MonoBehaviour
{
    public static GameFlow I { get; private set; }

    [Header("Links")]
    [SerializeField] private PlayerChargeShooter shooter;
    [SerializeField] private PlayerJumpMover mover;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    public GameState State { get; private set; }
    public bool IsPlaying => State == GameState.Playing;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void OnDestroy()
    {
        if (I == this) I = null;
    }

    void Start()
    {
        SetState(GameState.Menu);
    } 

    void SetState(GameState state)
    {
        State = state;

        SetActiveSafe(startPanel, state == GameState.Menu);
        SetActiveSafe(winPanel, state == GameState.Win);
        SetActiveSafe(losePanel, state == GameState.Lose);

        if (state == GameState.Menu)
            AudioManager.I?.ResetToMenu();

        if (state == GameState.Playing)
            AudioManager.I?.PlayBackground();

        if (state == GameState.Win)
            AudioManager.I?.PlayWin();

        if (state == GameState.Lose)
            AudioManager.I?.PlayLose();

        if (state == GameState.Win || state == GameState.Lose)
            AudioManager.I?.StopBackground();

        bool playing = state == GameState.Playing;

        if (shooter) shooter.SetInputEnabled(playing);

        if (!playing && mover)
            mover.StopMove();
    }

    static void SetActiveSafe(GameObject go, bool active)
    {
        if (!go) return;
        if (go.activeSelf == active) return;
        go.SetActive(active);
    }

    public void Play()
    {
        if (State != GameState.Menu) return;
        SetState(GameState.Playing);
    }

    public void Win()
    {
        if (!IsPlaying) return;
        SetState(GameState.Win);
    }

    public void Lose()
    {
        if (!IsPlaying) return;
        SetState(GameState.Lose);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}