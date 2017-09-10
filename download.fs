module RZ.App.Download

open System
open System.IO

module Video =
  type VideoInfo =
    { Bandwidth: int
      Resolution: int * int
      Path: string
    }
    with
      static member bandwidth x = x.Bandwidth

  let inline private array2Pair(a :'a[]) = a.[0], a.[1]
  let inline private mapBothPair (f: 'a -> 'b) (pair: 'a * 'a) = (fst pair |> f), (snd pair |> f)

  let private extractInfo (s :string) =
    let infos = s.Split(':').[1].Split(',')
                |> Seq.filter (fun s -> s.StartsWith("BANDWIDTH") || s.StartsWith("RESOLUTION"))
                |> Seq.map (fun i -> i.Split('=') |> array2Pair)
                |> Map.ofSeq
    { Bandwidth = Int32.Parse(infos.["BANDWIDTH"])
      Resolution = infos.["RESOLUTION"].Split('x') |> array2Pair |> mapBothPair Int32.Parse
      Path = null }

  let private toVideoInfo(inf, path) =
    printfn "info: %s, path: %s" inf path
    let info = inf |> extractInfo 
    in { info with Path = path }

  let parseVideoSelector :string[] -> VideoInfo seq =
    Seq.filter (fun s -> s.Length > 0 && (s.[0] = '/' || s.StartsWith("#EXT-X-STREAM-INF")))
        >> Seq.chunkBySize 2
        >> Seq.map array2Pair
        >> Seq.map toVideoInfo


module Twitter =
    open System.Diagnostics

    [<Literal>]
    let private FFmpegPath = @"C:\data\Utils\ffmpeg\bin\ffmpeg.exe" // get it from https://www.ffmpeg.org/download.html

    [<Literal>]
    let FFmegListFile = "list.txt"

    let downloadVideo(twitterUri, targetFilePath) =
        let videoSelectorContent = RZ.Net.openHttp twitterUri
        let videoSelector = videoSelectorContent |> Video.parseVideoSelector
        let bestResolution = videoSelector |> Seq.maxBy Video.VideoInfo.bandwidth
        let videoFile = Uri(twitterUri, bestResolution.Path)

        printfn "Loading...%A" videoFile

        let temp = Path.GetTempFileName()
        printfn "Temp at %s" temp

        let videoList =
          RZ.Net.openHttp videoFile 
          |> Seq.filter (fun s -> s.Length > 0 && s.[0] = '/')
          |> Seq.mapi (fun i s -> Uri(twitterUri, s), Path.Combine(temp, (sprintf "%d.ts" i)))
          |> Seq.toArray

        let currentDir = Directory.GetCurrentDirectory()

        File.Delete temp
        Directory.CreateDirectory temp |> ignore
        Directory.SetCurrentDirectory temp

        videoList |> Seq.iter (fun (src, dest) -> printf "\rsaving...%s" dest; RZ.Net.saveHttpTo(dest, src))

        printfn "\n"

        File.WriteAllLines( Path.Combine(temp, FFmegListFile), videoList |> Seq.map snd |> Seq.map (sprintf "file '%s'"))

        let concatArguments = sprintf "-f concat -safe 0 -i %s -c copy %s" FFmegListFile targetFilePath

        let concatProcess = ProcessStartInfo(FFmpegPath, concatArguments, UseShellExecute=false) |> Process.Start
        concatProcess.WaitForExit()

        Directory.SetCurrentDirectory currentDir
        Directory.Delete(temp, true)
