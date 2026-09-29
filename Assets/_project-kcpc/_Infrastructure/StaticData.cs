using ProjectKCPC.Scripts.Entity;
using UnityEngine;

[CreateAssetMenu(menuName = "StaticData")]
public class StaticData : ScriptableObject
{
    [field: SerializeField] public Player PlayerPrefab { get; private set; }
    [field: SerializeField] public string StartingSceneName { get; private set; } = "GameplayScene";
}
