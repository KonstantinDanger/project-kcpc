using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using Steamworks;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectKCPC.Scripts.Lobby
{
    public class Lobby : IStartable, IDisposable
    {
        private const string HostAddressKey = "HostAddress";

        public string LobbyName { get; private set; }
        public bool IsCreated { get; private set; }
        public CSteamID LobbyOwnerID => SteamMatchmaking.GetLobbyOwner(LobbyId);
        public CSteamID LobbyId { get; private set; }
        public int MaxPlayers { get; private set; }

        private FishySteamworks.FishySteamworks Steamworks { get; }
        private NetworkManager NetManager { get; }
        private SceneLoader SceneLoader { get; }
        private StaticData StaticData { get; }

        private Callback<LobbyCreated_t> LobbyCreated;
        private Callback<GameLobbyJoinRequested_t> JoinRequested;
        private Callback<LobbyEnter_t> LobbyEntered;

        public event Action<LobbyCreated_t> OnLobbyCreated;
        public event Action OnLobbyDisband;
        public event Action<GameLobbyJoinRequested_t> OnJoinRequested;
        public event Action<LobbyEnter_t> OnLobbyEnter;
        public event Action OnLobbyLeave;

        private ELobbyType _cachedLobbyType;

        [Inject]
        public Lobby(NetworkManager netManager, FishySteamworks.FishySteamworks steamworks, SceneLoader sceneLoader, StaticData staticData)
        {
            NetManager = netManager;
            Steamworks = steamworks;
            SceneLoader = sceneLoader;
            StaticData = staticData;
        }

        public void Start()
        {
            if (!SteamManager.Initialized)
                return;

            LobbyCreated = Callback<LobbyCreated_t>.Create(HandleLobbyCreated);
            JoinRequested = Callback<GameLobbyJoinRequested_t>.Create(HandleJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(HandleLobbyEnter);

            NetManager.ClientManager.OnClientConnectionState += HandleClientConnectionState;
        }

        public void Dispose()
        {
            NetManager.ClientManager.OnClientConnectionState -= HandleClientConnectionState;
        }

        //public void QuitGame()
        //{
        //    //if (IsMatchActive())
        //    //    return;

        //    if (IsCreated)
        //        Leave();

        //    Application.Quit();
        //}

        public IEnumerator Create(ELobbyType lobbyType, int maxPlayersAmount = 4)
        {
            if (IsCreated)
                yield break;

            bool succeed = false;
            bool failed = false;

            MaxPlayers = maxPlayersAmount;

            SteamMatchmaking.CreateLobby(lobbyType, MaxPlayers);

            yield return new WaitUntil(() => IsCreated);

            NetManager.ServerManager.OnServerConnectionState += HandleConnectionResult;

            Steamworks.SetClientAddress(SteamUser.GetSteamID().ToString());

            NetManager.ServerManager.StartConnection();
            NetManager.ClientManager.StartConnection();

            while (!succeed && !failed)
                yield return null;

            NetManager.ServerManager.OnServerConnectionState -= HandleConnectionResult;

            void HandleConnectionResult(ServerConnectionStateArgs args)
            {
                if (args.ConnectionState == LocalConnectionState.Stopped)
                    failed = true;
                else if (args.ConnectionState == LocalConnectionState.Started)
                    succeed = true;
            }

            _cachedLobbyType = lobbyType;

            //if (IsMatchActive())
            //    return;
        }

        public IEnumerator Disband()
        {
            //if (IsMatchActive())
            //    return;

            //if (!NetworkServer.active)
            //    return;

            if (!IsHost() || !IsCreated)
                yield break;

            SteamMatchmaking.LeaveLobby(LobbyId);
            ResetLobbyData();

            foreach (NetworkConnection connection in NetManager.ServerManager.Clients.Values.ToList())
            {
                if (connection.IsLocalClient)
                    continue;

                connection.Disconnect(false);
            }

            SteamMatchmaking.CreateLobby(_cachedLobbyType, MaxPlayers);
            yield return new WaitUntil(() => IsCreated);

            //StartCoroutine(DisbandAfterServerStopRoutine());
        }

        public void Leave()
        {
            //if (IsMatchActive())
            //{
            //    SendRequestLeaveDuringMatch();
            //    return;
            //}

            if (IsHost())
            {
                SteamMatchmaking.LeaveLobby(LobbyId);
                ResetLobbyData();
                NetManager.ServerManager.StopConnection(true);
                return;
            }

            SteamMatchmaking.LeaveLobby(LobbyId);
            ResetLobbyData();
            NetManager.ClientManager.StopConnection();

            //StartCoroutine(InvokeLeaveWhenClientDisconnect());
        }

        //private void SendRequestLeaveDuringMatch()
        //{
        //    if (IsHost())
        //        RequestLeaveDuringMatch();
        //    else
        //        CmdRequestLeaveDuringMatch();
        //}

        //private void RequestLeaveDuringMatch()
        //    => Events.InvokeMatchLeaveRequest(NetworkClient.connection.identity.netId);

        //[Command(requiresAuthority = false)]
        //private void CmdRequestLeaveDuringMatch()
        //    => RequestLeaveDuringMatch();

        public void Invite()
        {
            //if (IsMatchActive())
            //    return;

            SteamFriends.ActivateGameOverlayInviteDialogConnectString(LobbyId.ToString());
        }

        private void HandleClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                SteamMatchmaking.LeaveLobby(LobbyId);
                ResetLobbyData();
                SceneLoader.Load(StaticData.MainMenuScene);
            }
        }

        private void HandleLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
                return;

            LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            string pchValue = SteamUser.GetSteamID().ToString();

            SteamMatchmaking.SetLobbyData(
                LobbyId,
                HostAddressKey,
                pchValue);

            SteamMatchmaking.SetLobbyData(
                LobbyId,
                "name",
                SteamFriends.GetPersonaName().ToString() + "'s Lobby");

            IsCreated = true;

            //OnLobbyCreated?.Invoke(callback);
        }

        private void HandleJoinRequest(GameLobbyJoinRequested_t callback)
        {
            if (!SteamMatchmaking.RequestLobbyData(callback.m_steamIDLobby))
            {
                UnityEngine.Debug.Log($"Failed to connect to lobby with id \"{callback.m_steamIDLobby}\"");
                return;
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

            //OnJoinRequested.Invoke(callback);
        }

        private void HandleLobbyEnter(LobbyEnter_t callback)
        {
            LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            LobbyName = SteamMatchmaking.GetLobbyData(LobbyId, "name");

            if (IsHost())
            {
                OnLobbyEnter?.Invoke(callback);

                return;
            }

            string address = SteamMatchmaking.GetLobbyData(LobbyId, HostAddressKey);
            Steamworks.SetClientAddress(address);

            NetManager.ClientManager.StartConnection();

            //OnLobbyEnter?.Invoke(callback);

            IsCreated = true;

            //StartCoroutine(InvokeLobbyEnterWhenClientConnect(callback));
        }

        //private IEnumerator InvokeLobbyEnterWhenClientConnect(LobbyEnter_t callback)
        //{
        //    while (!NetworkClient.active)
        //        yield return null;

        //    yield return null;

        //    OnLobbyEnter.Invoke(callback);
        //}

        //private IEnumerator DisbandAfterServerStopRoutine()
        //{
        //    while (NetworkServer.active || NetworkClient.active)
        //        yield return null;

        //    yield return null;

        //    OnLobbyDisband?.Invoke();
        //}

        //private IEnumerator InvokeLeaveWhenClientDisconnect()
        //{
        //    while (NetworkClient.active)
        //        yield return null;

        //    yield return null;

        //    OnLobbyLeave?.Invoke();
        //}

        private bool IsHost()
            => LobbyOwnerID == SteamUser.GetSteamID();

        private void ResetLobbyData()
        {
            LobbyName = "";
            LobbyId = new CSteamID();
            IsCreated = false;
        }

        //private bool IsMatchActive()
        //{
        //    //MatchStatusReceiver matchStatus = null;

        //    try
        //    {
        //        matchStatus = ServiceLocator.Container.Resolve<MatchStatusReceiver>();
        //    }
        //    catch
        //    {
        //        return false;
        //    }

        //    return matchStatus.IsMatchActive;
        //}
    }
}
