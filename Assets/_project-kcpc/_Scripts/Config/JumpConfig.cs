using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/Jump")]
    public class JumpConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float JumpHeight { get; private set; } = 1.8f;
    }
}