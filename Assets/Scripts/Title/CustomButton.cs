using UnityEngine;
using UnityEngine.UI;

namespace Hackathon2023Winter.Title
{
    /// <summary>
    /// 自身の下にあるButtonにstateを反映させるButton
    /// </summary>
    public class CustomButton:Button
    {
        [SerializeField] private Button hideButton;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state,instant);
            //hideButtonを同じstateにする
            SetHideButtonState(state,instant);
        }

        private void SetHideButtonState(SelectionState state, bool instant)
        {
            if (!hideButton.gameObject.activeInHierarchy)
                return;

            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = hideButton.colors.normalColor;
                    transitionSprite = null;
                    triggerName = hideButton.animationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = hideButton.colors.highlightedColor;
                    transitionSprite = hideButton.spriteState.highlightedSprite;
                    triggerName = hideButton.animationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = hideButton.colors.pressedColor;
                    transitionSprite = hideButton.spriteState.pressedSprite;
                    triggerName = hideButton.animationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = hideButton.colors.selectedColor;
                    transitionSprite = hideButton.spriteState.selectedSprite;
                    triggerName = hideButton.animationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = hideButton.colors.disabledColor;
                    transitionSprite = hideButton.spriteState.disabledSprite;
                    triggerName = hideButton.animationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }

            switch (hideButton.transition)
            {
                case Transition.ColorTint:
                    StartHideButtonColorTween(tintColor * hideButton.colors.colorMultiplier, instant);
                    break;
                case Transition.SpriteSwap:
                    DoHideButtonSpriteSwap(transitionSprite);
                    break;
                case Transition.Animation:
                    TriggerHideButtonAnimation(triggerName);
                    break;
            }
        }
        
        private void StartHideButtonColorTween(Color targetColor, bool instant)
        {
            if (hideButton.targetGraphic == null)
                return;

            hideButton.targetGraphic.CrossFadeColor(targetColor, instant ? 0f : hideButton.colors.fadeDuration, true, true);
        }
        
        private void DoHideButtonSpriteSwap(Sprite newSprite)
        {
            if (hideButton.image == null)
                return;

            hideButton.image.overrideSprite = newSprite;
        }
        
        private void TriggerHideButtonAnimation(string triggername)
        {
#if PACKAGE_ANIMATION
            if (hideButton.transition != Transition.Animation || hideButton.animator == null || !hideButton.animator.isActiveAndEnabled || !hideButton.animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
                return;

            hideButton.animator.ResetTrigger(hideButton.animationTriggers.normalTrigger);
            hideButton.animator.ResetTrigger(hideButton.animationTriggers.highlightedTrigger);
            hideButton.animator.ResetTrigger(hideButton.animationTriggers.pressedTrigger);
            hideButton.animator.ResetTrigger(hideButton.animationTriggers.selectedTrigger);
            hideButton.animator.ResetTrigger(hideButton.animationTriggers.disabledTrigger);

            hideButton.animator.SetTrigger(triggername);
#endif
        }
        
        
    }
}