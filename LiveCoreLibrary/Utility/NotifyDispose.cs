using System;
using System.Collections.Generic;

namespace LiveCoreLibrary.Utility
{
    public class NotifyDispose<T> : IDisposable
    {
        private List<IObserver<T>> _observerListRef; // SimpleObserverが持つリスナ一覧への参照
        private IObserver<T> _targetObserver;

        // コンストラクタ
        public NotifyDispose(List<IObserver<T>> observerListRef, IObserver<T> targetObserver)
        {
            this._observerListRef = observerListRef;
            this._targetObserver = targetObserver;
        }

        // 削除処理
        public void Dispose()
        {
            if (this._observerListRef == null)
            {
                // 既に削除が終わっていたら何もしない
                return;
            }

            if (_observerListRef.IndexOf(_targetObserver) != -1)
            {
                // 監視中だったら、監視対象から外す
                _observerListRef.Remove(_targetObserver);
            }

            // 削除完了をマーキングする
            _observerListRef = null;
            _targetObserver = null;
        }
    }
}