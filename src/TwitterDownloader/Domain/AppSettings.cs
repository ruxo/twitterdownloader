using Microsoft.Extensions.Configuration;

namespace RZ.App.TwitterDownloader.Domain
{
    public class AppSettings
    {
        public AppSettings(IConfiguration config) {
            FFMpegPath = config["ffmpeg"];
            DefaultSaveLocation = config["defaultSaveLocation"];
        }

        public string FFMpegPath { get; }
        public string DefaultSaveLocation { get; }
    }
}
