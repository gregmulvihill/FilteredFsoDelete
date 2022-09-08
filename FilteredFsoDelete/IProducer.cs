namespace FilteredFsoDelete
{
    public interface IProducer<T, TCONTEXT> : IBase<TCONTEXT>
    {
        bool Produce(int instance, out T o);
    }
}