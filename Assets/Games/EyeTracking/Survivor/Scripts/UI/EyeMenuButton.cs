using System;
using UnityEngine;
using UnityEngine.UI;

public class EyeMenuButton : MonoBehaviour
{
    [SerializeField] private Text labelText;
    [SerializeField] private Image background;

    public Action OnSelected;

    public void SetLabel(string label)
    {
        labelText.text = label;
    }

    public void SetFocused(bool focused)
    {
        background.color = focused ? Color.yellow : Color.white;
    }

    public void Select()
    {
        OnSelected?.Invoke();
    }
}
