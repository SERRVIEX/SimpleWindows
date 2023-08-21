using System;

using UnityEngine;

public static class QuaternionExtensions
{
    /// <summary>
    /// Evaluates a rotation needed to be applied to an object positioned at sourcePoint to face destPoint
    /// </summary>
    /// <param name="sourcePoint">Coordinates of source point</param>
    /// <param name="destPoint">Coordinates of destionation point</param>
    /// <returns></returns>
    public static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
    {
        Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);

        float dot = Vector3.Dot(Vector3.forward, forwardVector);

        if (Mathf.Abs(dot - (-1.0f)) < 0.000001f)
            return new Quaternion(Vector3.up.x, Vector3.up.y, Vector3.up.z, 3.1415926535897932f);
        
        if (Mathf.Abs(dot - (1.0f)) < 0.000001f)
            return Quaternion.identity;

        float rotAngle = Mathf.Acos(dot);
        Vector3 rotAxis = Vector3.Cross(Vector3.forward, forwardVector);
        rotAxis = Vector3.Normalize(rotAxis);
        return CreateFromAxisAngle(rotAxis, rotAngle);
    }

    // just in case you need that function also
    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float halfAngle = angle * .5f;
        float s = (float)Math.Sin(halfAngle);
        Quaternion q;
        q.x = axis.x * s;
        q.y = axis.y * s;
        q.z = axis.z * s;
        q.w = (float)Math.Cos(halfAngle);
        return q;
    }
}
