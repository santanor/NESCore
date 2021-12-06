namespace NESCore;

public static class BitmapUtils
{
    public static int[] Resize(in int[] image, int destWidth, int destHeight)
    {
        var newImage = new int[destWidth * destHeight];
        var xRatio = (256 << 16) / destWidth + 1;
        var yRatio = (240 << 16) / destHeight + 1;

        for (var i = 0; i < destHeight; i++)
        for (var j = 0; j < destWidth; j++)
        {
            var xVal = (j * xRatio) >> 16;
            var yVal = (i * yRatio) >> 16;
            newImage[i * destWidth + j] = image[yVal * 256 + xVal];
        }

        return newImage;
    }
}