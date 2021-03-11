// Created by Takuya Isaki on 2021/03/09

using System;
using LiveCoreLibrary;
using UI.infrastracture;
using UnityEngine;

namespace UI.Presenter
{
    /// <summary>
    /// This screen display comment which is received server from unity runtime ui
    /// </summary>
    [Serializable]
    public class CommentScreen : ICommentScreen
    {
        [SerializeField] private UnityEngine.UI.InputField _inputField;
        [SerializeField] private UnityEngine.UI.Text _text;
        
        public string GetText()
        {
            var text = this._inputField.text;
            this._inputField.text = "";
            return text;
        }
        
        public void SetText(ChatMessage message)
            => _text.text += $"id :{message.id} messsage :{message.message}\n";
    }
}