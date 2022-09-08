namespace FilteredFsoDelete.ProducerConsumer
{
    public interface IConsumer<T, TCONTEXT> : IBase<TCONTEXT>
    {
        bool Consume(int instanceIndex, T item);
    }
}