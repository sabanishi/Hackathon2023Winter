using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Sabanishi.Common
{
    public static class ButtonExtensions
    {
        public static IObservable<Unit> SafeOnClickAsObservable(this Button button)
        {
            if (button == null)
            {
                Debug.Log("Buttonがnullです");
                return Observable.Empty<Unit>();
            }

            return button.OnClickAsObservable();
        }
    }
}