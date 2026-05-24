namespace Monod.MathModule;

public static class Lerp
{
    public static float Cubic(float t) => (float)Math.Pow(t, 3);
    public static double Cubic(double t) => Math.Pow(t, 3);
}
