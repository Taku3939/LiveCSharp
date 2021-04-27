namespace VLLLiveEngine.test
{
    public interface IGameHubReceiver
    {
        /// <summary>
        /// ルーム入室時のコールバック
        /// </summary>
        /// <param name="user"></param>
        void OnJoin(ulong user);

        void OnLeave(ulong id);
    }
}