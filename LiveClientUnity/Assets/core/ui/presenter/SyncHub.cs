using System;
using System.Collections.Generic;
using System.Linq;
using LiveClient;
using LiveCoreLibrary;
using MessagePack;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayState = LiveCoreLibrary.PlayState;

namespace UI.Hub
{
    /// <summary>
    /// 曲同期用UI
    /// </summary>
    public class SyncHub : MonoBehaviour
    {
        [SerializeField] private Button syncButton;
        
        private int sceneNumber = 0;
        private Client client;
        public double TimeCode;
        public int MusicNumber;
        public int State;
        public void Start()
        {
            client = VLLNetwork.Client;
            client.OnConnected.Subscribe(_ =>
                client.OnMessageReceived
                    .Where(e => e.Item1.type == typeof(MusicValue))
                    .Subscribe(e => this.Received(MessagePackSerializer.Deserialize<MusicValue>(e.Item2))));
            
            syncButton.onClick.AddListener(() => this.Send(new MusicValue(TimeCode, MusicNumber, State)));
        }

        private void Send(MusicValue musicValue) => client.SendAsync(musicValue);


        private readonly Queue<Action> unloadTask = new Queue<Action>();

        private async void Received(MusicValue t)
        {
            while (unloadTask.Count != 0)
                unloadTask?.Dequeue().Invoke();
            if (t.MusicNumber == 0) return;
            var s = Addressables.LoadSceneAsync(KeyCreator(t.MusicNumber), LoadSceneMode.Additive);
            var instance = await s.Task;
            var task = new Action(async () =>
            {
                var s = Addressables.UnloadSceneAsync(instance);
                var i = await s.Task;
                Debug.Log($"{i.Scene.name}をunloadしました");
            });
            unloadTask.Enqueue(task);
            var director = instance.Scene.GetRootGameObjects().First(x => x.name == "TL")
                .GetComponent<PlayableDirector>();
            director.time = t.TimeCode;

            if ((PlayState) t.State == PlayState.Paused) director.Pause();
            else if ((PlayState) t.State == PlayState.Playing) director.Play();

            Debug.Log($"{t.TimeCode}, 曲目{t.MusicNumber}を再生します");
        }

        private static string KeyCreator(int n) => $"Assets/Scenes/0{n}.unity";
    }
}