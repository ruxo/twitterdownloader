#r "System.Web.dll"
#load "net.fs"

open System

module Twitter =
    open System.Text.RegularExpressions

    let private VideoPlaylistUri tid = sprintf "https://twitter.com/i/videos/tweet/%s" tid

    let private DataConfigFinder = Regex("data-config=\"(.*)\"")

    let getVideoPlaylist(tid) =
        let getDataConfig content = 
            let m = content |> DataConfigFinder.Match

            assert m.Success

            m.Groups.[1].Value |> System.Web.HttpUtility.HtmlDecode

        let playlist = tid |> VideoPlaylistUri |> Uri |> RZ.Net.readHttpText

        playlist |> getDataConfig

#if INTERACTIVE

let twitterId = Environment.GetCommandLineArgs().[2]

printfn "%s" <| Twitter.getVideoPlaylist twitterId

#endif