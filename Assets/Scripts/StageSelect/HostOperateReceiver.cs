using System;
using UniRx;
using UnityEngine;

namespace Hackathon2023Winter.StageSelect
{
    /// <summary>
    /// StageSelectScreenでのHostの操作を受け取るクラス
    /// </summary>
    public class HostOperateReceiver : MonoBehaviour
    {
        private Subject<Unit> _tmpButtonSubject;
        public IObservable<Unit> OnTmpButtonClicked => _tmpButtonSubject;

        public void Setup()
        {
            _tmpButtonSubject = new();
        }

        public void Cleanup()
        {
            _tmpButtonSubject.Dispose();
        }

        public void OnReceiveTmpButtonClicked()
        {
            _tmpButtonSubject.OnNext(Unit.Default);
        }
    }
}