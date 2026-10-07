using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Movement")]
    public class MovementConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float MovementSpeed { get; private set; } = 80f;
        [field: SerializeField, Min(0f)] public float JumpHeight { get; private set; } = 2f;
    }
}