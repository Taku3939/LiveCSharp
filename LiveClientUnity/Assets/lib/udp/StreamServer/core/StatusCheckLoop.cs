using System.Threading.Tasks;
using EventServerCore;
using LoopLibrary;

namespace StreamServer
{
    public class StatusCheckLoop : BaseLoop<Unit>
    {
        private readonly IDataHolder _dataHolder;
        public StatusCheckLoop(IDataHolder dataHolder, int interval, string name = "Input")
            : base(interval, name)
        {
            _dataHolder = dataHolder;
        }

        protected override Task Update(int count)
        {
            Utility.PrintDbg($"Num clients: {_dataHolder.GetDict().Count.ToString()}");
            return Task.CompletedTask;
        }
    }
}