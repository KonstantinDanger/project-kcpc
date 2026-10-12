using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Gravity")]
    public class GravityConfig : ScriptableObject
    {
        [field: SerializeField, Range(-100f, 0f)] public float Gravity { get; private set; } = -9.81f;
        [field: SerializeField, Min(0)] public float MaxFallSpeed { get; private set; } = 20f;
    }
}