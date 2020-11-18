using System.Diagnostics;
using System.IO;

namespace Ext.Net.Benchmarks.Common
{
    public class TestRunner
    {
        private ProcessMonitor _monitor;
        private readonly object _locker = new object();

        public void LogTestRun(string test, int index)
        {
            if (string.IsNullOrEmpty(test))
            {
                return;
            }

            if (_monitor is null || _monitor.IsIdle)
            {
                lock (_locker)
                {
                    if (_monitor is null || _monitor.IsIdle)
                    {
                        Debug.WriteLine($">>> Starting Benchmark test: '{test}'");

                        if (_monitor != null)
                        {
                            _monitor.Dispose();
                        }

                        var fileName = Path.Combine(@"c:\app\results\" + test + index + ".csv");
                        var fileInfo = new FileInfo(fileName);

                        if (!Directory.Exists(fileInfo.DirectoryName))
                        {
                            Directory.CreateDirectory(fileInfo.DirectoryName);
                        }

                        _monitor = new ProcessMonitor(fileInfo.FullName);
                        _monitor.Start();
                    }
                }
            }

            _monitor.KeepAlive();
        }
    }
}
