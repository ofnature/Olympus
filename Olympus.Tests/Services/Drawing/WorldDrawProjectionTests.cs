using System.Numerics;
using Olympus.Services.Drawing;

namespace Olympus.Tests.Services.Drawing;

public class WorldDrawProjectionTests
{
    [Fact]
    public void HorizontalCirclePoint_ZeroAngle_IsNorthOfCenter()
    {
        var center = new Vector3(10f, 0f, 20f);
        var point = WorldDrawProjection.HorizontalCirclePoint(center, 5f, 0f);

        Assert.Equal(center.X, point.X, 3);
        Assert.Equal(center.Y, point.Y, 3);
        Assert.Equal(center.Z + 5f, point.Z, 3);
    }

    [Fact]
    public void SampleHorizontalRect_ProducesFourCorners()
    {
        Span<Vector3> points = stackalloc Vector3[4];
        WorldDrawProjection.SampleHorizontalRect(Vector3.Zero, 0f, 1f, 10f, points, out var count);

        Assert.Equal(4, count);
        Assert.NotEqual(points[0], points[1]);
        Assert.NotEqual(points[1], points[2]);
        Assert.NotEqual(points[2], points[3]);
    }

    [Fact]
    public void SampleHorizontalCircle_IncludesFullLoop()
    {
        Span<Vector3> points = stackalloc Vector3[WorldDrawProjection.DefaultCircleSegments + 1];
        WorldDrawProjection.SampleHorizontalCircle(Vector3.Zero, 3f, 8, points, out var count);

        Assert.Equal(9, count);
        Assert.Equal(3f, points[0].Z, 3);
        Assert.InRange(points[count - 1].Z, 2.99f, 3.01f);
    }
}
