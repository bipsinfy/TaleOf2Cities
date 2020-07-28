namespace TaleOf2Cities
{
    public class TaleConfiguration
    {
        public const string Position = "TaleSettings";
        public string InputFileName { get; set; }
        public string OutputFileNamePrefix { get; set; }
        public string SearchKeyWord { get; set; }
        public int KthTopOrderWordPosition { get; set; }

    }
}
