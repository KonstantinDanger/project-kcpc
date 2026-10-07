using ProjectKCPC.Scripts.Entity;
using UnityEngine;

[CreateAssetMenu(menuName = "StaticData")]
public class StaticData : ScriptableObject
{
    [field: SerializeField] public Player PlayerPrefab { get; private set; }
    [field: SerializeField] public string MainMenuScene { get; private set; } = "MainMenuScene";
    [field: SerializeField] public string GameScene { get; private set; } = "GameplayScene";
    [field: SerializeField] public string LobbyScene { get; private set; } = "LobbyScene";

    public static class Constants
    {
        public static string EmptyTag { get; private set; } = " ";
    }
}
