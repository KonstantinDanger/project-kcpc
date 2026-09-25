using UnityEngine;

[CreateAssetMenu(menuName = "StaticData")]
public class StaticData : ScriptableObject
{
    [field: SerializeField] public string SampleMessage { get; private set; }
}
