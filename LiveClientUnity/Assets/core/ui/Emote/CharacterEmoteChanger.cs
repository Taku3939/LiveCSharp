// Created by Takuya Isaki on $DATE$.
// 
// 

using System.Collections.Generic;
using LiveClient;
using LiveCoreLibrary;
using MessagePack;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace core.ui.emote
{
    /// <summary>
    /// 受信したデータからキャラクターの頭上にEmoteをポップアップさせるクラス
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterEmoteChanger : MonoBehaviour
    {
        private Animator _animator;
        private Client client;
        [SerializeField]private RawImage _rawImage;
        [SerializeField] private string trigger;
        [SerializeField] private Texture2D[] imageData;

        public void Start()
        {
            this._animator = this.GetComponent<Animator>();
            client = VLLNetwork.Client;

            client.OnMessageReceived?
                .Where(e => e.Item1.type == typeof(EmoteMessage))
                .Subscribe(e => this.Received(MessagePackSerializer.Deserialize<EmoteMessage>(e.Item2)));
        }
        public void Received(EmoteMessage message)
        {
            _rawImage.texture = getKey(message.key);
            _animator.SetTrigger(trigger);
        }

        public Texture2D getKey(int key)
        {
            return imageData[key];
        }
    }
}