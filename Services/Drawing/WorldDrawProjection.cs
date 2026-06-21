using System;
using System.Numerics;

namespace Olympus.Services.Drawing;

/// <summary>
/// World-space shape sampling for screen-space overlay projection.
/// Uses FFXIV horizontal plane (XZ) with Atan2(dx, dz) angle convention.
/// </summary>
public static class WorldDrawProjection
{
    public const int DefaultCircleSegments = 48;

    public static Vector3 HorizontalCirclePoint(Vector3 center, float radius, float angleRadians)
    {
        return new Vector3(
            center.X + radius * MathF.Sin(angleRadians),
            center.Y,
            center.Z + radius * MathF.Cos(angleRadians));
    }

    public static void SampleHorizontalArc(
        Vector3 center,
        float radius,
        float startRadians,
        float endRadians,
        int segments,
        Span<Vector3> output,
        out int count)
    {
        count = 0;
        if (segments < 1 || output.Length < segments + 1)
            return;

        var sweep = endRadians - startRadians;
        for (var i = 0; i <= segments; i++)
        {
            var t = segments == 0 ? 0f : i / (float)segments;
            output[count++] = HorizontalCirclePoint(center, radius, startRadians + sweep * t);
        }
    }

    public static void SampleHorizontalCircle(Vector3 center, float radius, int segments, Span<Vector3> output, out int count)
    {
        SampleHorizontalArc(center, radius, 0f, MathF.PI * 2f, segments, output, out count);
    }

    public static void SampleHorizontalRect(
        Vector3 origin,
        float heading,
        float halfWidth,
        float length,
        Span<Vector3> output,
        out int count)
    {
        var sinH = MathF.Sin(heading);
        var cosH = MathF.Cos(heading);
        var fwd = new Vector3(sinH * length, 0, cosH * length);
        var right = new Vector3(cosH * halfWidth, 0, -sinH * halfWidth);

        output[0] = origin - right;
        output[1] = origin + right;
        output[2] = origin + right + fwd;
        output[3] = origin - right + fwd;
        count = 4;
    }
}
