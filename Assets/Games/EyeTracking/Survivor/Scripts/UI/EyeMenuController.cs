using UnityEngine;

public class EyeMenuController : MonoBehaviour
{
    [SerializeField] private EyeMenuButton[] buttons;

    private int index;

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int captured = i;
            buttons[i].OnSelected = () => OnButtonSelected(captured);
        }

        UpdateVisuals();
    }

    private void Update()
    {
        if (SurvivorGameManager.Instance.State is not GameState.Menu and not GameState.GameOver)
        {
            return;
        }

        if (EyeInput.Instance.RightPressed)
        {
            index++;
            if (index >= buttons.Length) index = 0;
            UpdateVisuals();
        }

        if (EyeInput.Instance.LeftPressed)
        {
            index--;
            if (index < 0) index = buttons.Length - 1;
            UpdateVisuals();
        }

        if (BlinkDetector.Instance.DoubleBlinkPressed)
        {
            buttons[index].Select();
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetFocused(i == index);
        }
    }

    private void OnButtonSelected(int i)
    {
        string label = buttons[i].gameObject.name;

        if (label.Contains("Start"))
        {
            SurvivorGameManager.Instance.StartGame();
        }
        else if (label.Contains("Restart"))
        {
            SurvivorGameManager.Instance.StartGame();
        }
        else if (label.Contains("Quit"))
        {
            Application.Quit();
        }
    }
}
