using System.Threading.Tasks;
using EventServerCore;
using LoopLibrary;

namespace StreamServer
{
    public class StatusCheckLoop : BaseLoop<Unit>
    {
        private DataHolder _dataHolder;
        public StatusCheckLoop(DataHolder dataHolder, int interval, string name = "Input")
            : base(interval, name)
        {
            _dataHolder = dataHolder;
        }

        protected override async Task Update(int count)
        {
            Utility.PrintDbg($"Num clients: {_dataHolder.Users.Count}");
        }
    }
}