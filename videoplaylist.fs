module RZ.App.VideoPlaylist

open System

module Twitter =
    open System.Text.RegularExpressions
    open Newtonsoft.Json.Linq

    let private VideoPlaylistUri tid = sprintf "https://twitter.com/i/videos/tweet/%s" tid

    let private DataConfigFinder = Regex("data-config=\"(.*)\"", RegexOptions.Compiled)

    let getVideoPlaylist(tid) =
        let getDataConfig content = 
            let m = content |> DataConfigFinder.Match

            assert m.Success

            let dataconfigText = m.Groups.[1].Value |> System.Web.HttpUtility.HtmlDecode

            let dataconfig = JObject.Parse dataconfigText

            dataconfig.["video_url"].ToString()

        let playlist = tid |> VideoPlaylistUri |> Uri |> RZ.Net.readHttpText

        playlist |> getDataConfig
