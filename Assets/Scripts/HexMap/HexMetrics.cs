using UnityEngine;

public static class HexMetrics {
    public const float OUTER_RADIUS = 10f;
    public const float INNER_RADIUS = OUTER_RADIUS * 0.866025404f;
    public const float SOLID_FACTOR = 0.75f;
    public const float BLEND_FACTOR = 1f - SOLID_FACTOR;
    public const float ELEVATION_STEP = 5f;
    public const int TERRACES_PER_SLOPE = 2;
    public const int TERRACE_STEPS = TERRACES_PER_SLOPE * 2 + 1;
    public const float HORIZONTAL_TERRACE_STEP_SIZE = 1f / TERRACE_STEPS;
    public const float VERTICAL_TERRACE_STEP_SIZE = 1f / (TERRACES_PER_SLOPE + 1);

    private static Vector3[] corners ={
        new Vector3 (0f,0f, OUTER_RADIUS),
        new Vector3(INNER_RADIUS,0f, 0.5f * OUTER_RADIUS),
        new Vector3(INNER_RADIUS,0f, -0.5f * OUTER_RADIUS),
        new Vector3(0f,0f, -OUTER_RADIUS),
        new Vector3(-INNER_RADIUS,0f,-0.5f *OUTER_RADIUS),
        new Vector3(-INNER_RADIUS,0f,0.5f * OUTER_RADIUS),
        new Vector3(0f,0f,OUTER_RADIUS)
    };

    public static Vector3 GetFirstCorner(HexDirection direction) {
        return corners[(int) direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction) {
        return corners[((int) direction) + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction) {
        return corners[(int) direction] * SOLID_FACTOR;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction) {
        return corners[(int) (direction + 1)] * SOLID_FACTOR;
    }

    public static Vector3 GetBridge(HexDirection direction) {
        return (corners[(int) direction] + corners[(int) direction + 1])
            * BLEND_FACTOR;
    }

    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step) {
        var h = step * HORIZONTAL_TERRACE_STEP_SIZE;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;
        var v = ((step + 1) / 2) * VERTICAL_TERRACE_STEP_SIZE;
        a.y += (b.y - a.y) * v;
        return a;
    }

    public static Color TerraceLerp(Color a, Color b, int step) {
        var h = step * HORIZONTAL_TERRACE_STEP_SIZE;
        return Color.Lerp(a, b, h);
    }

    public static HexEdgeType GetEdgeType(int elevation1, int elevation2) {
        if (elevation1 == elevation2)
            return HexEdgeType.Flat;
        var delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1) {
            return HexEdgeType.Slope;
        }
        return HexEdgeType.Cliff;
    }
}