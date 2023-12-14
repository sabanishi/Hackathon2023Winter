using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sabanishi.Common
{
    /// <summary>
    /// Buttonに付属するTextの色を一緒に変更するButton
    /// </summary>
    public class CustomButton:Button
    {
        [SerializeField] private ColorBlock myColors;
        [SerializeField] private TMP_Text text;
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state,instant);
            switch (state)
            {
                case SelectionState.Normal:
                    text.color = myColors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    text.color = myColors.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    text.color = myColors.pressedColor;
                    break;
                case SelectionState.Selected:
                    text.color = myColors.selectedColor;
                    break;
                case SelectionState.Disabled:
                    text.color = myColors.disabledColor;
                    break;
                default:
                    text.color = Color.black;
                    break;
            }
        }
    }
}