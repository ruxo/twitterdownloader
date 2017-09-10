#r "System.Web.dll"
#r "packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#load "net.fs"

#load "net.fs"
#load "download.fs"
#load "videoplaylist.fs"

open System
open System.IO
open RZ.App.VideoPlaylist
open RZ.App.Download

let twitterId = Environment.GetCommandLineArgs().[2]

let targetFilePath = Path.Combine(Directory.GetCurrentDirectory(), sprintf "%s.mp4" twitterId)

let playlist = Twitter.getVideoPlaylist twitterId

printfn "Playlist at %s" playlist

Twitter.downloadVideo(Uri playlist, targetFilePath)
