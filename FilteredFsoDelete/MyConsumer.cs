using System.Diagnostics;
using System.IO;
using FilteredFsoDelete.ProducerConsumer;

namespace FilteredFsoDelete
{
    public class MyConsumer : IConsumer<string, AppSettings>
    {
        private AppSettings _settings;
        private Action<string> _log;
        private bool _demoMode;

        public void Init(AppSettings settings, Action<string> log, bool demoMode)
        {
            _settings = settings;
            _log = log;
            _demoMode = demoMode;

            _log($"{nameof(MyConsumer)} {Thread.CurrentThread.ManagedThreadId:N0} Init");
        }

        public bool Consume(int instanceIndex, string path)
        {
            //if (path.Contains("nuget", StringComparison.InvariantCultureIgnoreCase))
            //{
            //}

            _log($"{nameof(MyConsumer)} {Thread.CurrentThread.ManagedThreadId:N0} {path}");

            if (_demoMode)
            {
                return true;
            }

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                }
            }
            else
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }
            else
            {
                return false;
            }

            return true;

            //return FileOperationAPIWrapper.DeleteCompletelySilent(path);
            //return FileOperationAPIWrapper.MoveToRecycleBin(path);
        }

        public void Dispose()
        {
            _log($"{nameof(MyConsumer)} {Thread.CurrentThread.ManagedThreadId:N0} Done");
        }
    }
}
