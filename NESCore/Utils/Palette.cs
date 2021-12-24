using System.IO;

namespace NESCore;

public static class Palette
{
    private const string paletteFileName = "palette.pal";

    public static int[] Colours;

    static Palette()
    {
        HydratePalette();
    }

    /// <summary>
    /// Reads paletteFileName and hydrates the static array that contains the colours
    /// </summary>
    private static void HydratePalette()
    {
        var filePath = Directory.GetCurrentDirectory() +"/"+ paletteFileName;
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine("Can't find palette file");
        }
        
        // Each color is represented by 3 bytes. So to know how many colours are in the file
        // We simply divide the total size by 3. 

        using var fStream = File.Open(filePath, FileMode.Open);
        Colours = new int[fStream.Length / 3];

        // Each colour is 3 bytes, to read the file in 3 byte intervals 
        int r, g, b;
        for (var i = 0; i < Colours.Length; i++)
        {
            r = fStream.ReadByte();
            g = fStream.ReadByte();
            b = fStream.ReadByte();
            
            // Colour format is BGRA
            Colours[i] = (255 << 24) + (r << 16) + (g << 8) + b;
            
        }



    }
}