using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameHub.Survivor
{
    public class EyeMenuButton : MonoBehaviour
    {
        [SerializeField] private Text labelText;
        [SerializeField] private Image background;

        public event Action Selected;

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
            Selected?.Invoke();
        }
    }
}
