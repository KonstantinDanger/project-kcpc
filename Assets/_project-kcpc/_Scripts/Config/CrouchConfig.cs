using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Crouch")]
    public class CrouchConfig : ScriptableObject 
    {
        [field: SerializeField, Range(0f, 3f)] public float CrouchHeight { get; private set; } = 0.5f;
        [field: SerializeField, Min(0f)] public float CrouchAcceleration { get; private set; } = 0.03f;
        [field: SerializeField, Min(0f)] public float CrouchSpeed { get; private set; } = 5f;
    }
}