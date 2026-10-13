using UnityEngine;

namespace ProjectKCPC.Scripts
{
    public interface IExternalForceHandler 
    { 
        void UpdateExternalForce(float drag);
        void ApplyExternalForce(Vector3 force);
        void ApplyExternalForce(Vector3 direction, float force);
    }
}
