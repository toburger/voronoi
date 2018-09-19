namespace Voronoi

open System.Drawing

type DirectBitmap (width, height) =
    let bits = Array.zeroCreate (width * height)
    let bitsHandle =
        System.Runtime.InteropServices.GCHandle.Alloc(
            bits,
            System.Runtime.InteropServices.GCHandleType.Pinned
        )
    let bitmap =
        new Bitmap(
            width,
            height,
            width * 4,
            Imaging.PixelFormat.Format32bppPArgb,
            bitsHandle.AddrOfPinnedObject()
        )
    member self.Width = width
    member self.Bitmap = bitmap
    member self.Height = height
    member self.SetPixel(x, y, (color: Color)) =
        let index = x + (y * width)
        let col = color.ToArgb()
        bits.[index] <- col
    member self.GetPixel(x, y) =
        let index = x + (y * width)
        let col = bits.[index]
        Color.FromArgb(col)
    interface System.IDisposable with
        member self.Dispose() =
            bitmap.Dispose()
            bitsHandle.Free()


