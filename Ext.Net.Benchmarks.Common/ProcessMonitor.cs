using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ext.Net.Benchmarks.Common
{
    public sealed class ProcessMonitor : IDisposable
    {
        private readonly Process _process;
        private readonly string _filePath;
        private readonly int _periodMs;
        private readonly int _maxIdleMs;

        private readonly SemaphoreSlim _semaphore;
        private readonly StringBuilder _sb;
        private readonly int _maxThreads;

        private CancellationTokenSource _currentCts;
        private Task _currentTask;

        private DateTime _keepAliveTs;
        private DateTime _startTs;

        private TextWriter _out;

        public ProcessMonitor(string filePath, int periodMs = 200, int maxIdleMs = 1000)
        {
            ThreadPool.GetMaxThreads(out _maxThreads, out _);

            _filePath = filePath;
            _periodMs = periodMs;
            _maxIdleMs = maxIdleMs;

            _process = Process.GetCurrentProcess();
            _semaphore = new SemaphoreSlim(1, 1);
            _sb = new StringBuilder();
        }

        public async Task StartAsync()
        {
            await Synchronized(async () =>
            {
                await StopUnsafeAsync(true);

                var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                _out = new StreamWriter(stream, Encoding.UTF8, 4096, false);

                _currentCts = new CancellationTokenSource();
                var ct = _currentCts.Token;

                _currentTask = Task.Run(async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        await LogAsync();
                        await Task.Delay(_periodMs);
                    }

                    await StopAsync(false);
                });

                _keepAliveTs = DateTime.MinValue;
                _startTs = DateTime.UtcNow;

                KeepAlive();
            });
        }

        public async Task StopAsync(bool wait = true)
        {
            await Synchronized(async () =>
            {
                await StopUnsafeAsync(wait);
            });
        }

        private async Task StopUnsafeAsync(bool wait)
        {
            if (_currentTask != null)
            {
                _currentCts.Cancel();

                if (wait)
                {
                    await _currentTask;
                }

                _currentTask = null;

                _currentCts.Dispose();
                _currentCts = null;

                _keepAliveTs = DateTime.MinValue;

                await _out.FlushAsync();
                _out.Dispose();
            }
        }

        public void KeepAlive()
        {
            if ((DateTime.UtcNow - _keepAliveTs).TotalMilliseconds > _periodMs)
            {
                _keepAliveTs = DateTime.UtcNow;
                _currentCts?.CancelAfter(_maxIdleMs);
            }
        }

        private async Task LogAsync()
        {
            ThreadPool.GetAvailableThreads(out var avThreads, out _);

            _sb.Clear();

            _sb.Append((int)(DateTime.UtcNow - _startTs).TotalMilliseconds);
            _sb.Append(';');
            _sb.Append((int)_process.TotalProcessorTime.TotalMilliseconds);
            _sb.Append(';');
            _sb.Append(_process.PrivateMemorySize64);
            _sb.Append(';');
            _sb.Append(_process.WorkingSet64);
            _sb.Append(';');
            _sb.Append(GC.GetTotalMemory(false));
            _sb.Append(';');
            _sb.Append(GC.CollectionCount(0));
            _sb.Append(';');
            _sb.Append(GC.CollectionCount(1));
            _sb.Append(';');
            _sb.Append(GC.CollectionCount(2));
            _sb.Append(';');
            _sb.Append(_maxThreads - avThreads);

            await _out.WriteLineAsync(_sb.ToString());
        }

        private async Task Synchronized(Func<Task> action)
        {
            var isTaken = false;

            try
            {
                do
                {
                    try
                    {
                    }
                    finally
                    {
                        isTaken = await _semaphore.WaitAsync(TimeSpan.FromSeconds(1));
                    }
                }
                while (!isTaken);

                await action();
            }
            finally
            {
                if (isTaken)
                {
                    _semaphore.Release();
                }
            }
        }

        public void Dispose()
        {
            _currentCts?.Cancel();
            _semaphore.Dispose();
            _process.Dispose();

            if (_out != null)
            {
                _out.Flush();
                _out.Dispose();
            }
        }
    }
}
