using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public interface IRotatable
    {
        /// <summary>
        /// Rotation angle param
        /// </summary>
        event System.Action<float> OnRotate;
        Quaternion LookRotation { get; }
        Vector3 Forward { get; }
        void Rotate(Vector3 direction, float speed);
    }
}