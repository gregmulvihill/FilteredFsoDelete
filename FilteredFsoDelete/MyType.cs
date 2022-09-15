namespace FilteredFsoDelete
{
    public class MyType
    {
        public bool IsChecked { get; set; }
        public string RegularExpression { get; set; }

        public MyType()
        {
            IsChecked = true;
            RegularExpression = String.Empty;
        }

        public MyType(bool isChecked, string label)
        {
            IsChecked = isChecked;
            RegularExpression = label;
        }

        public override string ToString()
        {
            return $"{IsChecked}-{RegularExpression}";
        }
    }
}