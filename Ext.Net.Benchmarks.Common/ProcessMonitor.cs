using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Ext.Net.Benchmarks.Common
{
    public sealed class ProcessMonitor : IDisposable
    {
        private readonly Process _process;
        private readonly string _filePath;
        private readonly int _periodMs;
        private readonly int _maxIdleMs;

        private readonly StringBuilder _sb;
        private readonly int _maxThreads;
        private readonly object _locker = new object();

        private RecurringJob _job;

        private DateTime _keepAliveTs;
        private DateTime _startTs;
        private double _startProcMs;

        private TextWriter _out;

        public bool IsIdle => _out is null;

        public ProcessMonitor(string filePath, int periodMs = 500, int maxIdleMs = 2000)
        {
            ThreadPool.GetMaxThreads(out _maxThreads, out _);

            _filePath = filePath;
            _periodMs = periodMs;
            _maxIdleMs = maxIdleMs;

            _process = Process.GetCurrentProcess();
            _sb = new StringBuilder();
        }

        public void Start()
        {
            lock (_locker)
            {
                Stop();

                var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
                _out = new StreamWriter(stream, Encoding.UTF8, 4096, false);

                LogHeader();

                _job = new RecurringJob(
                    _periodMs,
                    () => { Log(); },
                    () => { Stop(); }
                );

                _keepAliveTs = DateTime.MinValue;
                _startTs = DateTime.UtcNow;
                _startProcMs = _process.TotalProcessorTime.TotalMilliseconds;

                KeepAlive();
            }
        }

        public void Stop()
        {
            if (_job is null)
            {
                return;
            }

            lock (_locker)
            {
                if (_job != null)
                {
                    _job.Dispose();
                    _job = null;

                    _keepAliveTs = DateTime.MinValue;

                    _out.Flush();
                    _out.Dispose();
                    _out = null;
                }
            }
        }

        public void KeepAlive()
        {
            if ((DateTime.UtcNow - _keepAliveTs).TotalMilliseconds > _periodMs)
            {
                _keepAliveTs = DateTime.UtcNow;
                _job?.CancelAfter(_maxIdleMs);
            }
        }

        private void Log()
        {
            _sb.Clear();
            _process.Refresh();

            ThreadPool.GetAvailableThreads(out var avThreads, out _);

            _sb.Append((int)(DateTime.UtcNow - _startTs).TotalMilliseconds);
            _sb.Append(';');
            _sb.Append((int)(_process.TotalProcessorTime.TotalMilliseconds - _startProcMs));
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
            _sb.Append('\n');

            _out.Write(_sb.ToString());
        }

        private void LogHeader()
        {
            _sb.Clear();

            _sb.Append("OS Version: " + Environment.OSVersion);
            _sb.Append('\n');

            _sb.Append("CLR Version: " + Environment.Version);
            _sb.Append('\n');

            _sb.Append("Processor Count: " + Environment.ProcessorCount);
            _sb.Append('\n');

            _sb.Append('\n', 21);

            _sb.Append("TS (ms)");
            _sb.Append(';');
            _sb.Append("CPU time (ms)");
            _sb.Append(';');
            _sb.Append("Private Mem (B)");
            _sb.Append(';');
            _sb.Append("Working Set (B)");
            _sb.Append(';');
            _sb.Append("Allocated Mem (B)");
            _sb.Append(';');
            _sb.Append("Gen0 collections");
            _sb.Append(';');
            _sb.Append("Gen1 collections");
            _sb.Append(';');
            _sb.Append("Gen2 collections");
            _sb.Append(';');
            _sb.Append("ThreadPool size");
            _sb.Append('\n');

            _out.Write(_sb.ToString());
        }

        public void Dispose()
        {
            _job?.Dispose();
            _process.Dispose();

            if (_out != null)
            {
                _out.Flush();
                _out.Dispose();
            }
        }

        private class RecurringJob : IDisposable
        {
            private readonly CancellationTokenSource _cts;
            private readonly int _periodMs;
            private readonly Action _job;
            private readonly Action _finalizer;

            public RecurringJob(int periodMs, Action job, Action finalizer)
            {
                _periodMs = periodMs;
                _job = job;
                _finalizer = finalizer;
                _cts = new CancellationTokenSource();

                var thread = new Thread(TickHandler);
                thread.Start();
            }

            public void CancelAfter(int delayMs)
            {
                _cts.CancelAfter(delayMs);
            }

            private void TickHandler()
            {
                while (true)
                {
                    if (_cts.IsCancellationRequested)
                    {
                        _finalizer();
                        return;
                    }

                    _job();

                    Thread.Sleep(_periodMs);
                }
            }

            public void Dispose()
            {
                _cts.Cancel();
            }
        }
    }
}
