using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiveCoreLibrary
{
    public abstract class BaseLoop<T>
    {
        private readonly int _interval;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _count;
        public CancellationTokenSource Cts => _cts;

        protected BaseLoop(int interval)
        {
            _interval = interval;
        }

        public void Run()
        {
            Task.Run(() => Loop(_cts.Token), _cts.Token);
        }

        private async Task<Result<T>> Loop(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    var delay = Task.Delay(_interval, token);

                    await Update(_count);
                    _count++;

                    await delay;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Loop canceled");
                return new Result<T>(false);
            }
            catch (OperationCompletedException<Result<T>> e)
            {
                Console.WriteLine("Loop completed");
                return e.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on loop\n" + e);
                return new Result<T>(false);
            }
        }

        protected abstract Task Update(int count);

        public void Done(T result)
        {
            throw new OperationCompletedException<Result<T>>(new Result<T>(true, result));
        }

        public void Cancel()
        {
            throw new OperationCanceledException();
        }
    }
}