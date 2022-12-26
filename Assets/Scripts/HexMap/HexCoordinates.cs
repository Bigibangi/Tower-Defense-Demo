using UnityEngine;

[System.Serializable]
public struct HexCoordinates {

    #region Editor

    [SerializeField]
    private int _x,_z;

    #endregion Editor

    #region Properties

    public int X {
        get { return _x; }
    }

    public int Z {
        get {
            return _z;
        }
    }

    public int Y {
        get {
            return -X - Z;
        }
    }

    #endregion Properties

    public HexCoordinates(int x, int z) {
        _x = x;
        _z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z) {
        return new HexCoordinates(x - z / 2, z);
    }

    public override string ToString() {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    public string ToStringOnSeparateLine() {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }

    public static HexCoordinates FromPosition(Vector3 position) {
        var x = position.x / (HexMetrics.INNER_RADIUS * 2f);
        var y = -x;
        var offset = position.z / (HexMetrics.OUTER_RADIUS * 3f);
        x -= offset;
        y -= offset;
        var iX = Mathf.RoundToInt(x);
        var iY = Mathf.RoundToInt(y);
        var iZ = Mathf.RoundToInt(-x - y);
        if (iX + iY + iZ != 0) {
            var dX = Mathf.Abs(x - iX);
            var dY = Mathf.Abs(y - iY);
            var dZ = Mathf.Abs(-x -y - iZ);
            if (dX > dY && dX > dZ) {
                iX = -iY - iZ;
            }
            else if (dZ > dY) {
                iZ = -iX - iY;
            }
        }
        return new HexCoordinates(iX, iZ);
    }
}