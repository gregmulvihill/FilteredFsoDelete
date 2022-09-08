using System.Diagnostics;

namespace FilteredFsoDelete
{
    public class MyConsumer : IConsumer<string, SettingsEx>
    {
        private SettingsEx _settings;
        private Action<string> _log;
        private bool _demoMode;

        public void Init(SettingsEx settings, Action<string> log, bool demoMode)
        {
            _settings = settings;
            _log = log;
            _demoMode = demoMode;

            _log($"{nameof(MyConsumer)} {Thread.CurrentThread.ManagedThreadId:N0} Init");
        }

        public bool Consume(int instanceIndex, string path)
        {
            _log($"{nameof(MyConsumer)} {Thread.CurrentThread.ManagedThreadId:N0} {path}");

            if (_demoMode)
            {
                return true;
            }

            return FileOperationAPIWrapper.MoveToRecycleBin(path);
        }

        public void Dispose()
        {
            _log($"{nameof(MyConsumer)} {Thread.CurrentThread.ManagedThreadId:N0} Done");
        }
    }
}
