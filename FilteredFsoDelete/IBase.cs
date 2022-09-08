namespace FilteredFsoDelete
{
    public interface IBase<TCONTEXT> : IDisposable
    {
        void Init(TCONTEXT context, Action<string> log, bool demoMode);
    }
}