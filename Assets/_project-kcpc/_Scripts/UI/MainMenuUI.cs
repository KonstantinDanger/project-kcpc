using ProjectKCPC.Scripts.Lobby;
using Steamworks;
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

        [Inject]
        private void Construct(Lobby lobby)
        {
            _lobby = lobby;
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
            _lobby.Create(_lobbyType);
        }
    }
}
