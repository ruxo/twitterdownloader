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

let args = Environment.GetCommandLineArgs()
let currentDir = Directory.GetCurrentDirectory()

let twitterId = if args.Length > 2 then args.[2] else null

let download tid =
  let targetFilePath = Path.Combine(currentDir, sprintf "%s.mp4" tid)
  let playlist = Twitter.getVideoPlaylist tid
  Twitter.downloadVideo(Uri playlist, targetFilePath)

let promptTwitterId() =
  printf "Input Twitter ID: "
  Console.ReadLine()

if String.IsNullOrEmpty twitterId then
  let mutable tid = promptTwitterId()
  while not <| String.IsNullOrEmpty tid do
    download tid
    tid <- promptTwitterId()
else
  download twitterId