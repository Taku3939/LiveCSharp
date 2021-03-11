// Created by Takuya Isaki on 2021/03/09

using LiveCoreLibrary;

namespace UI.infrastracture
{
    public interface ICommentScreen
    {

        string GetText();
        /// <summary>
        /// This method process chat message received from server
        /// </summary>
        /// <param name="message">message is received from server</param>
        void SetText(ChatMessage message);
    }
}