using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// コルーチンの待機を補助するオブジェクト
/// </summary>
public class UnityWebRequestAwaiter : INotifyCompletion
{
    /// <summary>
    /// 対象となるコルーチン
    /// </summary>
    private readonly UnityWebRequestAsyncOperation target;

    private Action continuation;

    /// <summary>
    /// 非同期処理が完了しているか
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// 結果のオブジェクトを取得する場合に利用する
    /// 今回は結果の値がないのでvoid
    /// </summary>
    public void GetResult()
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation coroutine)
    {
        target = coroutine;
        target.completed += OnRequestCompleted;
    }

    /// <summary>
    /// await開始時に実行される関数
    /// </summary>
    /// <param name="continuation">「await以降の処理」を扱うAction</param>
    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
    }
    private void OnRequestCompleted(AsyncOperation obj)
    {
        continuation();
    }
}

public static class ExtensionMethods
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation coroutine)
    {
        return new UnityWebRequestAwaiter(coroutine);
    }
}