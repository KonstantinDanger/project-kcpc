using ProjectKCPC.Scripts;
using ProjectKCPC.Scripts.Lobby;
using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ProjectKCPC
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private ELobbyType _lobbyType = ELobbyType.k_ELobbyTypePublic;

        private Lobby _lobby;
        private StaticData _staticData;
        private SceneLoader _sceneLoader;

        [Inject]
        private void Construct(Lobby lobby, StaticData staticData, SceneLoader sceneLoader)
        {
            _lobby = lobby;
            _staticData = staticData;
            _sceneLoader = sceneLoader;
        }

        private void OnEnable()
        {
            _startButton.onClick.AddListener(HandleStart);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveListener(HandleStart);
        }

        private void HandleStart()
        {
            StartCoroutine(HandleStartRoutine());
        }

        private IEnumerator HandleStartRoutine()
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; 
            yield return _lobby.Create(_lobbyType);
            _sceneLoader.Load(_staticData.NextSceneFromMenu);
            _sceneLoader.Unload(currentScene);
        }
    }
}
