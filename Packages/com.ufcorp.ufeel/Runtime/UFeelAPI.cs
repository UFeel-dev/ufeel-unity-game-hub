
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UFeel
{
    public partial class UFeelAPI : MonoBehaviour
    {
        private static UFeelAPI? _instance;

        private readonly CancellationTokenSource cancellation = new();
        private readonly SemaphoreSlim processorGate = new(1, 1);
        private static Task processTask;

        private bool isQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (FindAnyObjectByType<UFeelAPI>() != null)
                return;

            GameObject obj = new("UFeelAPI");
            obj.AddComponent<UFeelAPI>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            processTask = ProcessLoopAsync(cancellation.Token);
            DontDestroyOnLoad(gameObject);

            Debug.Log("UFeelAPI initialized");
        }

        private async Task ProcessLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    bool updated = false;

                    await processorGate.WaitAsync(token);

                    try
                    {
                        if (isQuitting)
                            return;
                        updated = UFeelProcessor.Update();
                        RulesProcessor.UpdateRules();
                    }
                    finally
                    {
                        processorGate.Release();
                    }

                    if (!updated)
                    {
                        await Task.Delay(50, token);
                        continue;
                    }

                    await Task.Delay(33, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private void FixedUpdate()
        {
            if (isQuitting)
                return;

            RulesProcessor.UpdateRules();
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
            Debug.Log("Game stopping - releasing UFeel resources");

            UFeelProcessor.Shutdown();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
