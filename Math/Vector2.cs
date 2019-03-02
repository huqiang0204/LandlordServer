using System;
using System.Collections.Generic;
using System.Text;

public struct Vector2
{
    public float x;
    public float y;
    public Vector2(float a, float b) { x = a; y = b; }
    public static bool operator !=(Vector2 v1, Vector2 v2)
    {
        if (v1.x != v2.x)
            return true;
        if (v1.y != v2.y)
            return true;
        return false;
    }
    public static bool operator ==(Vector2 v1, Vector2 v2)
    {
        if (v1.x != v2.x)
            return false;
        if (v1.y != v2.y)
            return false;
        return true;
    }
    public static Vector2 operator +(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x + v2.x, v1.y + v2.y);
    }
    public static Vector2 operator -(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x - v2.x, v1.y - v2.y);
    }
    public static float Dot(Vector2 left, Vector2 right)
    {
        return (left.x * right.x) + (left.y * right.y);
    }
    public static Vector2 operator *(Vector2 v1, float v2)
    {
        return new Vector2(v1.x * v2, v1.y * v2);
    }

    public static readonly Vector2 zero = new Vector2();
    public float Length
    {
        get { return MathF.Sqrt((x * x) + (y * y)); }
    }
    public static Vector2 Move(Vector2 v,float len)
    {
        if (v.x == 0 & v.y == 0)
            return v;
        float sx = v.x * v.x + v.y * v.y;
        float r = MathF.Sqrt(len * len / sx);
        return new Vector2(v.x*r,v.y*r);
    }
    public override string ToString()
    {
        return "Vector2 {x:" + x.ToString() + " y:" + y.ToString() + "}";
    }
}
public struct Point2
{
    public int x;
    public int y;
    public Point2(int a, int b) { x = a; y = b; }
}