using System.Collections.Immutable;
using System.Linq;
using RZ.Foundation.Extensions;
using RZ.Foundation.Types;
using static RZ.Foundation.Prelude;

namespace RZ.App.TwitterDownloader.Domain
{
    public sealed class VideoInfo
    {
        public VideoInfo(int bandwidth, (int,int) resolution, string path) {
            Bandwidth = bandwidth;
            Resolution = resolution;
            Path = path;
        }
        public int Bandwidth { get; }
        public (int Horz,int Vert) Resolution { get; }
        public string Path { get; }
    }

    public static class Video
    {
        public static Iter<VideoInfo> ParseVideoSelector(Iter<string> m3u8Content) =>
            Iter(m3u8Content.Where(s => s.Length > 0 && (s[0] == '/' || s.StartsWith("#EXT-X-STREAM-INF")))
                .Batch(2)
                .Select(i => Array2Pair(i.ToArray()))
                .Select(pairs => ToVideoInfo(pairs.Item1, pairs.Item2)));

        static VideoInfo ToVideoInfo(string streamInfo, string path) {
            var info = ExtractInfo(streamInfo);
            return new VideoInfo(info.Bandwidth, info.Resolution, path);
        }

        static (int Bandwidth,(int,int) Resolution) ExtractInfo(string streamInfo) {
            var infos = (from prop in streamInfo.Split(':')[1].Split(',')
                        where prop.StartsWith("BANDWIDTH") || prop.StartsWith("RESOLUTION")
                        select prop.Split('=')
                        ).ToImmutableDictionary(k => k[0], v => v[1]);
            return (int.Parse(infos["BANDWIDTH"]), Array2Pair(infos["RESOLUTION"].Split('x').Select(int.Parse).ToArray()));
        }

        static (T, T) Array2Pair<T>(T[] a) => (a[0], a[1]);
    }
}
