using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Running, Crashed }

// The rules of the run: the speed grows with time, the distance counts up, a crash stops everything (R restarts).
// F1 is a test key: god mode, nothing can kill the player.
[DefaultExecutionOrder(-50)]
public class GameManager : MonoBehaviour
{
    [SerializeField] private float startSpeed = 11f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float rampSeconds = 150f;      // time until the speed is at its maximum

    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Running;
    public float Speed { get; private set; }
    public float ElapsedTime { get; private set; }
    public float Distance { get; set; }
    public bool GodMode { get; private set; }

    // 0 at the start, 1 when the game is as hard as it gets
    public float Difficulty => Mathf.Clamp01(ElapsedTime / rampSeconds);

    public event Action Crashed;

    private void Awake()
    {
        Instance = this;
        Speed = startSpeed;
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;

        if (State == GameState.Crashed)
        {
            if (kb != null && kb.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (kb != null && kb.f1Key.wasPressedThisFrame) GodMode = !GodMode;

        ElapsedTime += Time.deltaTime;
        // fast at the start, slower later: the player has time to get used to every new speed
        float ease = 1f - Mathf.Pow(1f - Difficulty, 2f);
        Speed = Mathf.Lerp(startSpeed, maxSpeed, ease);
    }

    public void Crash()
    {
        if (State == GameState.Crashed || GodMode) return;

        State = GameState.Crashed;
        Speed = 0f;
        Crashed?.Invoke();
    }
}
