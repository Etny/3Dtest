using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace _3Dtest
{
    class Camera
    {
        public Vector3 Position = new Vector3(0.0f, 0.0f, 3.0f);
        public Vector3 Front = new Vector3(0.0f, 0.0f, -1.0f);
        public Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);

        public float Pitch = 0.0f;
        public float Yaw = -90.0f;

        public Vector3 Target { get; set; }


        // public Vector3 GetDirection() => Vector3.Normalize(Position - Target);
        // public Vector3 GetXAxis() => Vector3.Normalize(Vector3.Cross(new Vector3(0, 1, 0), GetDirection()));
        //  public Vector3 GetYAxis() => Vector3.Normalize(Vector3.Cross(GetDirection(), GetXAxis()));

        public Matrix4x4 LookAt() => Matrix4x4.CreateLookAt(Position, Position + GetDirection(), Up);
        public Vector3 GetDirection()
        {
            float x = (float)(Math.Cos(Util.ToRadians(Yaw)) * Math.Cos(Util.ToRadians(Pitch)));
            float y = (float)Math.Sin(Util.ToRadians(Pitch));
            float z = (float)(Math.Sin(Util.ToRadians(Yaw)) * Math.Cos(Util.ToRadians(Pitch)));
            return Vector3.Normalize(new Vector3(x, y, z));
        }

        public void SetDirection(Vector3 dir)
        {
            Yaw = Util.ToDegrees((float)Math.Atan2(dir.Y, dir.X));
            Pitch = Util.ToDegrees((float)-Math.Asin(dir.X));
        }
        

    }
}
