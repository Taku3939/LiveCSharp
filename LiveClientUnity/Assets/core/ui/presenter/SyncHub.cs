using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LiveClient;
using LiveCoreLibrary;
using MessagePack;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Hub
{
    /// <summary>
    /// 曲同期用UI
    /// </summary>
    public class SyncHub : MonoBehaviour
    {
        [SerializeField]private string Key = $"Assets/Scenes/01.unity";
        private MusicValue value;
        private Client client;
        public void Start()
        {
            client = VLLNetwork.Client;
            client.OnConnected.Subscribe(_ =>
            {
                Task.Delay(100);
                client.SendAsync(MessageParser.EncodeCustom(MethodType.Get, new LiveCoreLibrary.Unit()));
                Debug.Log("メッセージを送信します");
                client.OnMessageReceived
                    .Where(e => e.Item1.type == typeof(MusicValue))
                    .Subscribe(e => this.Received(MessagePackSerializer.Deserialize<MusicValue>(e.Item2)));
            });
        }

        private void Send(MusicValue musicValue) => client.SendAsync(musicValue);


        private readonly Queue<Action> unloadTask = new Queue<Action>();

        private async void Received(MusicValue t)
        {

            var currentTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            var offset = currentTime - t.StartTimeCode;
            Debug.Log("current time : " + currentTime + "\n offset" + offset);
            if (offset < 0)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(-offset)).Subscribe(async _ => { await Load(t, 0);});
            }
            else
            {
                await Load(t, offset);
            }
        }

        /// <summary>
        /// 曲の非同期ロード
        /// </summary>
        /// <param name="t">曲の情報</param>
        /// <param name="offset">曲を開始するオフセット(Milliseconds)</param>
        public async Task Load(MusicValue t, double offset)
        {
            Debug.Log("offset : " + offset);
            //現在の曲のアンロード
            while (unloadTask.Count != 0)
                unloadTask?.Dequeue().Invoke();

            //シーンのロード
            var s = Addressables.LoadSceneAsync(Key, LoadSceneMode.Additive);
            var instance = await s.Task;
            
            //アンロードタスクの作成
            var task = new Action(async () =>
            {
                var s = Addressables.UnloadSceneAsync(instance);
                var i = await s.Task;
                Debug.Log($"{i.Scene.name}をunloadしました");
            });
            unloadTask.Enqueue(task);
            
            //タイムラインの取得
            var director = instance.Scene.GetRootGameObjects().First(x => x.name == "TL").GetComponent<PlayableDirector>();
            director.time = offset / 1000d;

            director.Play();
            // if ((PlayState) t.State == PlayState.Paused) director.Pause();
            // else if ((PlayState) t.State == PlayState.Playing) director.Play();

            Debug.Log($"{t.StartTimeCode} : 再生します");
        }
    }
}