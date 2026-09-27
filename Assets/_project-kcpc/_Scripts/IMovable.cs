using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public interface IMovable
    {
        Vector3 Velocity { get; }

        void Move(Vector3 direction, float speed, float smoothness = 0.03f);
    }
}
