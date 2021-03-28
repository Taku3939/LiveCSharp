using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StreamServer;
using UnityEngine;
using UnityEngine.Serialization;

namespace Udp
{
    public class ReceiveEntry : MonoBehaviour
    {
        private InputLoop input;
        private StatusCheckLoop _statusCheckLoop;
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        [SerializeField] private UdpSocketHolder udpSocketHolder;
        [SerializeField] private DataHolder dataHolder;
        [SerializeField] private RemotePlayerSpawner playerSpawner;

        private Task delay = Task.Delay(100);

        async Task OnEnable()
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            playerSpawner.TaskScheduler = scheduler;
            dataHolder.Initialize();
            await delay;
            input = new InputLoop(udpSocketHolder.UdpClient, dataHolder, playerSpawner, 5);
            input.Start();
            _statusCheckLoop = new StatusCheckLoop(dataHolder, 5000);
            var _ = _statusCheckLoop.Run();
            _cancellationTokenSources.Add(_statusCheckLoop.Cts);
        }

        private async void OnDisable()
        {
            await delay;
            input.Stop();
            await Task.Delay(10);
            udpSocketHolder.TryClose();
            foreach (var cts in _cancellationTokenSources)
            {
                cts.Cancel();
            }
        }
    }
}