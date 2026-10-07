using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VContainer;
using Steamworks;
using FishNet;

namespace ProjectKCPC.Scripts.UI
{
    public class LobbyView : UIView
    {
        [SerializeField] private TextMeshProUGUI _lobbyNameText;
        [SerializeField] private ELobbyType _lobbyType;

        [Header("Buttons")]
        [SerializeField] private Button _returnToGameButton;
        [SerializeField] private Button _quitGameButton;
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _inviteButton;
        [SerializeField] private Button _disbandButton;
        [SerializeField] private Button _leaveButton;

        private Lobby.Lobby _lobby;
        private SceneLoader _sceneLoader;
        private StaticData _staticData;

        [Inject]
        public void Construct(Lobby.Lobby lobby, SceneLoader sceneLoader, StaticData staticData)
        {
            _lobby = lobby;
            _sceneLoader = sceneLoader;
            _staticData = staticData;

            //_lobby.OnJoinRequested += HandleJoinRequest;
            _lobby.OnLobbyEnter += HandleLobbyEntered;

            _quitGameButton.onClick.AddListener(HandleQuitToMainMenu);

            _startGameButton.onClick.AddListener(HandleStartGame);
            _inviteButton.onClick.AddListener(HandleInvite);
            _leaveButton.onClick.AddListener(HandleLeaveLobby);
            _disbandButton.onClick.AddListener(HandleDisbandLobby);

            HandleUIChange();
        }

        public void OnDestroy()
        {
            //_lobby.OnJoinRequested -= HandleJoinRequest;
            _lobby.OnLobbyEnter -= HandleLobbyEntered;

            _quitGameButton.onClick.RemoveListener(HandleQuitToMainMenu);

            _startGameButton.onClick.RemoveListener(HandleStartGame);
            _inviteButton.onClick.RemoveListener(HandleInvite);
            _leaveButton.onClick.RemoveListener(HandleLeaveLobby);
            _disbandButton.onClick.RemoveListener(HandleDisbandLobby);
        }

        protected override void OnEnable()
        {
            if (_lobby == null)
                return;

            HandleUIChange();

            //base.OnEnable();
        }

        private void HandleQuitToMainMenu()
        {
            HandleLeaveLobby();

            //if (IsLobbyOwner())
            //    _lobby.Disband();
            //_lobby.QuitGame();
        }

        private void HandleStartGame()
        {
            _sceneLoader.Load(_staticData.GameScene);

            //Events.InvokeStartGame();
            //HandleUIChange();
        }

        private void HandleInvite()
            => _lobby.Invite();

        private void HandleDisbandLobby()
        {
            _lobby.Disband();
            _lobby.Create(_lobbyType);
            HandleUIChange();
        }

        private void HandleLeaveLobby()
        {
            _sceneLoader.Load(_staticData.MainMenuScene);
            _lobby.Leave();
            HandleUIChange();
        }

        private void HandleJoinRequest(GameLobbyJoinRequested_t callback)
            => HandleUIChange();

        private void HandleLobbyEntered(LobbyEnter_t callback)
            => HandleUIChange();

        private void HandleUIChange()
        {
            _lobbyNameText.text = string.IsNullOrEmpty(_lobby.LobbyName) ? "Offline" : _lobby.LobbyName;

            bool playersConnected = InstanceFinder.ServerManager.Clients.Count == _lobby.MaxPlayers;

            SetActive(_returnToGameButton, true);

            SetActive(_quitGameButton, !IsMissionGoing());

            SetActive(_startGameButton, IsLobbyOwner() && _lobby.IsCreated && !IsMissionGoing());

            if (_startGameButton.gameObject.activeInHierarchy)
                _startGameButton.enabled = playersConnected;

            SetActive(_inviteButton, _lobby.IsCreated && IsLobbyOwner() && !playersConnected && !IsMissionGoing());

            SetActive(_disbandButton, _lobby.IsCreated && IsLobbyOwner() && !IsMissionGoing());

            SetActive(_leaveButton, _lobby.IsCreated && (IsLobbyOwner() && IsMissionGoing() || !IsLobbyOwner()));
        }

        private void SetActive(Button btn, bool active)
            => btn.gameObject.SetActive(active);

        private bool IsLobbyOwner()
        {
            CSteamID ownerID = _lobby.LobbyOwnerID;
            CSteamID localPlayerID = SteamUser.GetSteamID();
            return ownerID == localPlayerID;
        }

        private bool IsMissionGoing()
            => false;
    }
}
