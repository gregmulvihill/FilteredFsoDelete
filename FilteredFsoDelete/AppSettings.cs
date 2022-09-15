namespace FilteredFsoDelete
{
    public class AppSettings
    {
        public string TargetDirectory { get; set; }
        public IEnumerable<MyType> FilesKeep { get; set; }
        public IEnumerable<MyType> DirectoriesKeep { get; set; }
        public IEnumerable<MyType> FilesDelete { get; set; }
        public IEnumerable<MyType> DirectoriesDelete { get; set; }
    }
}
