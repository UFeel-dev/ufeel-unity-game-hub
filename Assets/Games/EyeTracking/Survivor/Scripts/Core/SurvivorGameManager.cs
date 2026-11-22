using System;
using UnityEngine;

public class SurvivorGameManager : MonoBehaviour
{
    public static SurvivorGameManager Instance { get; private set; }

    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject hudUI;

    public GameState State { get; private set; }

    public event Action<GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        State = GameState.Menu;
    }

    private void OnEnable()
    {
        StateChanged += HandleState;
    }

    private void OnDisable()
    {
        StateChanged -= HandleState;
    }

    private void HandleState(GameState state)
    {
        menuUI.SetActive(state is GameState.Menu or GameState.GameOver);
        hudUI.SetActive(state == GameState.Playing);
    }

    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    public void ReturnToMenu()
    {
        ChangeState(GameState.Menu);
    }

    private void ChangeState(GameState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(State);
    }
}

// TODO: !!! implement the scene in unity !!!
// ! -> remember to add asmdef to make ufeel available in Input
