using UnityEngine;

namespace ProjectKCPC.Scripts.Extensions
{
    public static class Extensions
    {
        public static Vector3 WithX(this Vector3 v, float overrideX) => new(overrideX, v.y, v.z);
        public static Vector3 WithY(this Vector3 v, float overrideY) => new(v.x, overrideY, v.z);
        public static Vector3 WithZ(this Vector3 v, float overrideZ) => new(v.x, v.y, overrideZ);
    }
}
