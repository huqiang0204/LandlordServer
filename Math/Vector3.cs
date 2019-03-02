using System;
using System.Collections.Generic;
using System.Text;

public struct Vector3
{
    public float x;
    public float y;
    public float z;
    public Vector3(float a, float b, float c) { x = a; y = b; z = c; }
    public static Vector3 One { get { return new Vector3(1, 1, 1); } }
    public bool IsZero()
    {
        if (x == 0 & y == 0 & z == 0)
            return true;
        return false;
    }
    public static bool operator !=(Vector3 v1, Vector3 v2)
    {
        if (v1.x != v2.x)
            return true;
        if (v1.y != v2.y)
            return true;
        if (v1.z != v2.z)
            return true;
        return false;
    }
    public static bool operator ==(Vector3 v1, Vector3 v2)
    {
        if (v1.x != v2.x)
            return false;
        if (v1.y != v2.y)
            return false;
        if (v1.z != v2.z)
            return false;
        return true;
    }
    public static Vector3 operator +(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }
    public static Vector3 operator -(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }
    public static Vector3 operator *(Vector3 lhs, float rhs)
    {
        Vector3 r = new Vector3();
        r.x = lhs.x * rhs;
        r.y = lhs.y * rhs;
        r.z = lhs.z * rhs;
        return r;
    }
    public static Vector3 operator *( float rhs, Vector3 lhs)
    {
        Vector3 r = new Vector3();
        r.x = lhs.x * rhs;
        r.y = lhs.y * rhs;
        r.z = lhs.z * rhs;
        return r;
    }
    public static Vector3 operator /(Vector3 lhs, float rhs)
    {
        Vector3 r = new Vector3();
        r.x = lhs.x / rhs;
        r.y = lhs.y / rhs;
        r.z = lhs.z / rhs;
        return r;
    }
    public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
    {
        Vector3 r = new Vector3();
        r.x = lhs.x * rhs.x;
        r.y = lhs.y * rhs.y;
        r.z = lhs.z * rhs.z;
        return r;
    }
    public static Vector3 Cross(ref Vector3 left, ref Vector3 right)
    {
        return new Vector3(
            (left.x * right.z) - (left.z * right.y),
            (left.z * right.x) - (left.x * right.z),
            (left.x * right.y) - (left.y * right.x));
    }
    public static Vector3 Cross(Vector3 left, Vector3 right)
    {
        return  Cross(ref left, ref right);
    }
    public override string ToString()
    {
        return "Vector3 {x:" + x.ToString() + " y:" + y.ToString() + " z:" + z.ToString() + "}";
    }
    public static readonly Vector3 zero=new Vector3();
    public static readonly Vector3 forward = new Vector3(0,0,1);
    public static float Distance(Vector3 v1,Vector3 v2)
    {
        float x = v1.x - v2.x;
        x *= x;
        float y = v1.y - v2.y;
        y *= y;
        float z = v1.z - v2.z;
        z *= z;
        return MathF.Sqrt(x+y+z);
    }
    public static implicit operator Vector4(Vector3 v4) { return new Vector4(v4.x, v4.y, v4.z, 0); }
    public static implicit operator Vector2(Vector3 v4) { return new Vector2(v4.x, v4.y); }
    public static float Dot(Vector3 left, Vector3 right)
    {
        return (left.x * right.x) + (left.y * right.y) + (left.z * right.z);
    }
    public float Length
    {
        get { return MathF.Sqrt((x * x) + (y * y) + (z * z)); }
    }
    public Vector3 Normalize
    {
        get
        {
            Vector3 v = forward;
            float length = Length;
            if (length > 0)
            {
                float inv = 1.0f / length;
                v.x = x * inv;
                v.y = y * inv;
                v.z = z * inv;
            }
            return v;
        }
    }
    public static Vector3 Move(Vector3 v, float len)
    {
        if (v.x == 0 & v.y == 0&v.z==0)
            return v;
        float sx = v.x * v.x + v.y * v.y+v.z*v.z;
        float r = MathF.Sqrt(len * len / sx);
        return new Vector3(v.x * r, v.y * r,v.z*r);
    }
}