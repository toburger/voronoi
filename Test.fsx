open System.IO

#load "DirectBitmap.fs"
#load "Program.fs"

open Voronoi.Program

let imageWidth = 1900
let imageHeight = 1200
let nSites = 500

#time "on"

time "IMPERATIVE" (fun () -> generateVoronoiImp (imageWidth, imageHeight) nSites)
|> save (Path.Combine(__SOURCE_DIRECTORY__, "test1.png"))

time "FUNCITONAL" (fun () -> generateVoronoiFunc (imageWidth, imageHeight) nSites)
|> save (Path.Combine(__SOURCE_DIRECTORY__, "test2.png"))
