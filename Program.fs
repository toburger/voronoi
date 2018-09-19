module Voronoi.Program

open System.Drawing
open System.IO

let rnd =
    let random = System.Random()
    fun maxInclusive ->
        random.Next(0, maxInclusive + 1)

let dot x y =
    x*x + y*y

let getEncoder (format: Imaging.ImageFormat) =
    Imaging.ImageCodecInfo.GetImageEncoders()
    |> Array.find (fun c -> c.FormatID = format.Guid)

let generateVoronoi
        nSites
        (imageWidth, imageHeight)
        (randomColors: Color array)
        (sx: int array)
        (sy: int array) =
    //use bitmap = new Bitmap((imageWidth: int), (imageHeight: int))
    use bitmap = new DirectBitmap(imageWidth, imageHeight)
    for x in 0..imageWidth-1 do
        for y in 0..imageHeight-1 do
            let mutable dMin = dot imageWidth imageHeight
            let mutable sMin = 0
            for s in 0..nSites-1 do
                let d = dot (sx.[s]-x) (sy.[s]-y)
                if d < dMin then
                    sMin <- s
                    dMin <- d
            bitmap.SetPixel(x, y, randomColors.[sMin])
    let encoder = Imaging.Encoder.Quality
    let encoderParams = new Imaging.EncoderParameters(1)
    encoderParams.Param.[0] <- new Imaging.EncoderParameter(encoder, 100L)
    use stream = new MemoryStream()
    //bitmap.Save(stream, Imaging.ImageFormat.Png)
    let pngEncoder = getEncoder Imaging.ImageFormat.Png
    bitmap.Bitmap.Save(stream, pngEncoder, encoderParams = encoderParams)
    stream.ToArray()

let randomSites nSites (imageWidth, imageHeight) =
    let rnd = System.Random(hash System.DateTime.Now.Ticks)
    let sx = Array.zeroCreate nSites
    let sy = Array.zeroCreate nSites
    for i in 0..nSites-1 do
        sx.[i] <- rnd.Next(imageWidth)
        sy.[i] <- rnd.Next(imageHeight)
    sx, sy

let save filename bytes =
    File.WriteAllBytes(filename, bytes)

let generate
        (imageWidth, imageHeight)
        (sites: int array)
        (sx: int array)
        (sy: int array) =
    let xs = Array.init imageWidth id
    let ys = Array.init imageHeight id
    Array.allPairs xs ys
    |> Array.Parallel.map (fun (x, y) ->
        let sMin =
            sites
            |> Array.minBy (fun d ->
                dot (sx.[d] - x) (sy.[d] - y))
        (x, y), sites.[sMin])

let randomColors nSites =
    Array.init nSites (fun _ ->
        Color.FromArgb(255, rnd 255, rnd 255, rnd 255))

let encodeBitmap (bitmap: Bitmap) =
    use stream = new MemoryStream()
    bitmap.Save(stream, Imaging.ImageFormat.Png)
    stream.ToArray()

let colorize
        (imageWidth: int, imageHeight: int)
        (randomColors: Color array)
        (xs: ((int * int) * int) array) =
    //let bitmap = new Bitmap(imageWidth, imageHeight)
    let bitmap = new DirectBitmap(imageWidth, imageHeight)
    for ((x, y), v) in xs do
        bitmap.SetPixel(x, y, randomColors.[v])
    bitmap.Bitmap

let inline flip f a b = f b a

let generateVoronoiFunc size nSites =
    let randomColors = randomColors nSites
    let xs, xy = randomSites nSites size
    let sites = Array.init nSites id
    generate size sites xs xy
    |> colorize size randomColors
    |> flip using encodeBitmap

let generateVoronoiImp size nSites =
    let xs, xy = randomSites nSites size
    let randomColors' = randomColors nSites
    generateVoronoi nSites size randomColors' xs xy

let time caption f =
    let sw = System.Diagnostics.Stopwatch.StartNew()
    let res = f ()
    printfn "[%s]: %O" caption sw.Elapsed
    res

#if !INTERACTIVE
open Argu

type Arguments =
    | [<Mandatory; AltCommandLine "-w">] Width of int
    | [<Mandatory; AltCommandLine "-h">] Height of int
    | [<Mandatory; AltCommandLine "-n"; AltCommandLine "-s">] Sites of int
    interface IArgParserTemplate with
        member self.Usage =
            match self with
            | Width _ -> "The width of the image to generate"
            | Height _ -> "The height of the image to generate"
            | Sites _ -> "The number of sites to generate"

[<EntryPoint>]
let main args =
    let parser = ArgumentParser.Create<Arguments>(errorHandler = ProcessExiter())
    let result = parser.ParseCommandLine(args)

    let imageWidth = result.GetResult <@ Width @>
    let imageHeight = result.GetResult <@ Height @>
    let nSites = result.GetResult <@ Sites @>
    let outputDir = System.Environment.CurrentDirectory

    time "IMPERATIVE" (fun () -> generateVoronoiImp (imageWidth, imageHeight) nSites)
    |> save (Path.Combine(outputDir, "test1.png"))

    time "FUNCTIONAL" (fun () -> generateVoronoiFunc (imageWidth, imageHeight) nSites)
    |> save (Path.Combine(outputDir, "test2.png"))

    0
#endif
