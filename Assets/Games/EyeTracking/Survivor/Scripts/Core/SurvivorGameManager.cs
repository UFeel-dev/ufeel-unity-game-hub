using System;
using GameHub.Common;
using UnityEngine;

namespace GameHub.Survivor
{
    public class SurvivorGameManager : MonoBehaviour
    {
        [SerializeField] private GameObject menuUI;
        [SerializeField] private GameObject hudUI;

        public event Action<GameState> StateChanged;

        public static SurvivorGameManager? Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Menu;

        public bool IsPlaying => State == GameState.Playing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ApplyState();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void StartGame()
        {
            GameSounds.Play(SoundCue.Start);
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
                return;

            State = state;
            ApplyState();
            StateChanged?.Invoke(State);
        }

        private void ApplyState()
        {
            if (menuUI != null)
                menuUI.SetActive(!IsPlaying);

            if (hudUI != null)
                hudUI.SetActive(IsPlaying);
        }
    }
}
