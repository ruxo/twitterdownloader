#r "System.Web.dll"
#r "packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#load "net.fs"

open System

module Twitter =
    open System.Text.RegularExpressions
    open Newtonsoft.Json.Linq

    let private VideoPlaylistUri tid = sprintf "https://twitter.com/i/videos/tweet/%s" tid

    let private DataConfigFinder = Regex("data-config=\"(.*)\"")

    let getVideoPlaylist(tid) =
        let getDataConfig content = 
            let m = content |> DataConfigFinder.Match

            assert m.Success

            let dataconfigText = m.Groups.[1].Value |> System.Web.HttpUtility.HtmlDecode

            let dataconfig = JObject.Parse dataconfigText

            dataconfig.["video_url"].ToString()

        let playlist = tid |> VideoPlaylistUri |> Uri |> RZ.Net.readHttpText

        playlist |> getDataConfig

#if INTERACTIVE

let twitterId = Environment.GetCommandLineArgs().[2]

printfn "%s" <| Twitter.getVideoPlaylist twitterId

#endif