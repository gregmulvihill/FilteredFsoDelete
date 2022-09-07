namespace FilteredFsoDelete
{
    internal class MyType
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }

        public override string ToString()
        {
            return String.Join(", ", GetType().GetProperties().Select(x => x.Name + "=" + x.GetValue(this)));
        }
    }
}