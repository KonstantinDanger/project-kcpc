using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Crouch")]
    public class CrouchConfig : ScriptableObject 
    {
        [field: SerializeField, Range(0f, 3f)] public float CrouchHeight { get; private set; } = 0.8f;
        [field: SerializeField, Min(0f)] public float CrouchTransitionSpeed { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float CrouchRapidTransitionSpeed { get; private set; } = 12f;
        [field: SerializeField, Min(0f)] public float CrouchSpeed { get; private set; } = 5f;
    }
}