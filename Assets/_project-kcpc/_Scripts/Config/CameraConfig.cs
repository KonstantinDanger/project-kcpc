using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Camera")]
    public class CameraConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float MinRotationAngle { get; private set; } = -90f;
        [field: SerializeField, Min(0f)] public float MaxRotationAngle { get; private set; } = 90f;
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; private set; } = 90f;
    }
}