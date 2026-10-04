using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Camera")]
    public class CameraConfig : ScriptableObject
    {
        [field: SerializeField] public float MinRotationAngle { get; private set; } = -90f;
        [field: SerializeField] public float MaxRotationAngle { get; private set; } = 90f;
        [field: SerializeField] public float RotationSpeed { get; private set; } = 90f;
    }
}