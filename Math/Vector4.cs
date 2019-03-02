using System;
using System.Collections.Generic;
using System.Text;

public struct Vector4
{
    public float x;
    public float y;
    public float z;
    public float w;
    public Vector4(float a, float b, float c, float d) { x = a; y = b; z = c; w = d; }

    public static bool operator !=(Vector4 v1, Vector4 v2)
    {
        if (v1.x != v2.x)
            return true;
        if (v1.y != v2.y)
            return true;
        if (v1.z != v2.z)
            return true;
        if (v1.w != v2.w)
            return true;
        return false;
    }
    public static bool operator ==(Vector4 v1, Vector4 v2)
    {
        if (v1.x != v2.x)
            return false;
        if (v1.y != v2.y)
            return false;
        if (v1.z != v2.z)
            return false;
        if (v1.w != v2.w)
            return false;
        return true;
    }
    public static Vector4 operator +(Vector4 v1, Vector4 v2)
    {
        return new Vector4(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
    }
    public static Vector4 operator -(Vector4 v1, Vector4 v2)
    {
        return new Vector4(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
    }
    public static Vector4 operator *(Vector4 lhs, float rhs)
    {
        Vector4 r = new Vector4();
        r.x = lhs.x * rhs;
        r.y = lhs.y * rhs;
        r.z = lhs.z * rhs;
        r.w = lhs.w * rhs;
        return r;
    }
    public static Vector4 zero { get { return new Vector4(); } }
    public static implicit operator Vector3(Vector4 v4) { return new Vector3(v4.x, v4.y,v4.z); }
  
}