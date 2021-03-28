using System.Threading.Tasks;
using StreamServer.Model;

namespace StreamServer
{
    public interface IPlayerSpawner
    {
        /// <summary>
        /// ユーザを生成する
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        Task Spawn(MinimumAvatarPacket packet);
    }
}