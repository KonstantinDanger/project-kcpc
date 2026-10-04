using ProjectKCPC.Scripts.Entity;
using UnityEngine;

[CreateAssetMenu(menuName = "StaticData")]
public class StaticData : ScriptableObject
{
    [field: SerializeField] public Player PlayerPrefab { get; private set; }
    [field: SerializeField] public string StartingSceneName { get; private set; } = "GameplayScene";
    [field: SerializeField] public string GameSceneName { get; private set; } = "GameplayScene";
    [field: SerializeField] public string NextSceneFromMenu { get; private set; } = "LobbyScene";

    public static class Constants
    {
        public static string EmptyTag { get; internal set; } = "";
    }
}
