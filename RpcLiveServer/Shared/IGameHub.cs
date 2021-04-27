using System.Threading.Tasks;
using MagicOnion;

namespace VLLLiveEngine.test
{
    public interface IGameHub : IStreamingHub<IGameHub, IGameHubReceiver>
    {
        /// <summary>
        /// ルームへの参加
        /// </summary>
        /// <returns></returns>
        Task<Player[]> JoinAsync(ulong id);

        Task LeaveAsync(ulong id);
    }
}