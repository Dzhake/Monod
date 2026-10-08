namespace Monod.Graphics;

public class Atlas
{
    public MTexture[] Images;
    public Point ImageSize;

    public Atlas(MTexture source, Point imageSize)
    {
        ImageSize = imageSize;
        int imagesInARow = source.Width / imageSize.X;
        int imagesInAColumn = source.Height / imageSize.Y;
        Images = new MTexture[imagesInARow * imagesInAColumn];
        Point startAt = source.Region?.Location ?? Point.Zero;

        for (int k = 0; k < imagesInAColumn; k++)
        {
            for (int i = 0; i < imagesInARow; i++)
            {
                Images[i + (k * imagesInARow)] = new(source.Base, new(startAt, imageSize));
                startAt.X += imageSize.X;
            }
            startAt.Y += imageSize.X;
            startAt.X -= imageSize.X * imagesInAColumn;
        }

    }
}
