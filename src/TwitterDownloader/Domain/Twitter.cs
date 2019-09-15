using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using RZ.App.TwitterDownloader.Extensions;
using RZ.Foundation;
using static RZ.Foundation.Prelude;

namespace RZ.App.TwitterDownloader.Domain
{
    public static class FileSystem
    {
        public interface ITempDirectory : IDisposable
        {
            string GetFilePath(string name);
        }
        public static ITempDirectory GetTempDirectory() {
            var temp = Path.GetTempFileName();
            File.Delete(temp);
            return new TempDirectory(Directory.CreateDirectory(temp));
        }
        sealed class TempDirectory : ITempDirectory
        {
            readonly string currentDirectory;
            readonly DirectoryInfo dir;
            public TempDirectory(DirectoryInfo dir) {
                this.dir = dir;
                currentDirectory = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(dir.FullName);
            }
            public void Dispose() {
                Directory.SetCurrentDirectory(currentDirectory);
                dir.Delete(recursive: true);
            }
            public string GetFilePath(string name) => Path.Combine(dir.FullName, name);

        }
    }
    public static class Twitter
    {
        const string FFmegListFile = "list.txt";
        const int BufferSize = 64_000;

        public static async Task<Option<VideoInfo>> DownloadVideo(string ffmpegPath, string m3u8Uri, string targetFilePath) {
            var twitterVideoRoot = m3u8Uri.ResetToRoot();
            var videoContent = await ReadM3u8File(m3u8Uri);

            var videoInfos = Video.ParseVideoSelector(Iter(videoContent)).ToArray();
            var selectedVideo = videoInfos.MaxBy(v => v.Bandwidth);

            videoContent = await selectedVideo.GetAsync(async selected => await ReadM3u8File(twitterVideoRoot.Clone().AppendPathSegment(selected.Path)),
                                                        () => Task.FromResult(videoContent));

            using var temp = FileSystem.GetTempDirectory();

            var videoList = GetVideoFileList(twitterVideoRoot, videoContent)
                .Select((videoUrl, i) => (Url: videoUrl, Target: temp.GetFilePath($"{i}.ts")))
                .ToArray();

            await Task.WhenAll(from i in videoList select SaveVideo(i.Url, i.Target));

            await File.WriteAllLinesAsync(temp.GetFilePath(FFmegListFile), from i in videoList select $"file '{i.Target}'");

            var args = $"-f concat -safe 0 -i {FFmegListFile} -c copy {targetFilePath}";
            var mpegProcess = Process.Start(new ProcessStartInfo(ffmpegPath, args) { UseShellExecute = false });
            mpegProcess.WaitForExit();

            return selectedVideo;
        }

        static async Task<IEnumerable<string>> ReadM3u8File(string uri) {
            var content = await uri.GetStringAsync();
            return content.Split('\n');
        }

        static IEnumerable<Url> GetVideoFileList(Url twitterVideoRoot, IEnumerable<string> m3u8Content) =>
            from line in m3u8Content
            where line.Length > 0 && line[0] == '/'
            select twitterVideoRoot.Clone().AppendPathSegment(line);

        static DirectoryInfo GetTempDirectory() {
            var temp = Path.GetTempFileName();
            File.Delete(temp);
            return Directory.CreateDirectory(temp);
        }

        static async Task SaveVideo(Url source, string targetFile) {
            using var ss = await source.GetStreamAsync();
            using var ts = File.Create(targetFile, BufferSize);

            var buffer = new byte[BufferSize];

            var n = ss.Read(buffer, 0, BufferSize);
            while(n > 0) {
                ts.Write(buffer, 0, n);
                n = ss.Read(buffer, 0, BufferSize);
            }
        }
    }
}
