namespace ImageDownloaderApp.Models
{
    public class Input
    {
        public int Count { get; set; }
        public int Parallelism { get; set; }
        public string SavePath { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty;
    }
}
