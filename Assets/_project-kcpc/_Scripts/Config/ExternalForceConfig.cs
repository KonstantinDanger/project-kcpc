using UnityEngine;

namespace ProjectKCPC.Scripts.Config
{
    [CreateAssetMenu(menuName = "Config/External force")]
    public class ExternalForceConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float Drag { get; private set; }
    }
}