using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Movement")]
    public class MovementConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float WalkSpeed { get; private set; } = 80f;
        [field: SerializeField, Min(0f)] public float JumpHeight { get; private set; } = 2f;
        [field: SerializeField, Min(0f)] public float SprintSpeed { get; private set; } = 3.2f;
        [field: SerializeField, Min(0f)] public float MovementSmoothness { get; private set; } = 0.03f;
    }
}