namespace FilteredFsoDelete.ProducerConsumer
{
    public interface IBase<TCONTEXT> : IDisposable
    {
        void Init(TCONTEXT context, Action<string> log, bool demoMode);
    }
}