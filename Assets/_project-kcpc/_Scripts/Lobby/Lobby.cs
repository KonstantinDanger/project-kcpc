using FishNet.Managing;
using LiteNetLib;
using Steamworks;
using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectKCPC.Scripts.Lobby
{
    public class Lobby : IStartable
    {
        private const string HostAddressKey = "HostAddress";

        public string LobbyName { get; private set; }
        public bool IsCreated { get; private set; }
        public CSteamID LobbyOwnerID => SteamMatchmaking.GetLobbyOwner(LobbyId);
        public CSteamID LobbyId { get; private set; }
        public int MaxPlayers { get; private set; }
        private FishySteamworks.FishySteamworks Steamworks { get; }
        private NetworkManager NetManager { get; }

        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> JoinRequested;
        protected Callback<LobbyEnter_t> LobbyEntered;

        public event Action<LobbyCreated_t> OnLobbyCreated;
        public event Action OnLobbyDisband;
        public event Action<GameLobbyJoinRequested_t> OnJoinRequested;
        public event Action<LobbyEnter_t> OnLobbyEnter;
        public event Action OnLobbyLeave;

        [Inject]
        public Lobby(NetworkManager netManager, FishySteamworks.FishySteamworks steamworks)
        {
            NetManager = netManager;
            Steamworks = steamworks;
        }

        public void Start()
        {
            if (!SteamManager.Initialized)
                return;

            LobbyCreated = Callback<LobbyCreated_t>.Create(HandleLobbyCreated);
            JoinRequested = Callback<GameLobbyJoinRequested_t>.Create(HandleJoinRequest);
            LobbyEntered = Callback<LobbyEnter_t>.Create(HandleLobbyEnter);
        }

        public void QuitGame()
        {
            //if (IsMatchActive())
            //    return;

            if (IsCreated)
                Leave();

            Application.Quit();
        }

        public void Create(ELobbyType lobbyType, int maxPlayersAmount = 4)
        {
            //if (IsMatchActive())
            //    return;

            SteamMatchmaking.CreateLobby(lobbyType, maxPlayersAmount);
            MaxPlayers = maxPlayersAmount;
        }

        public void Disband()
        {
            //if (IsMatchActive())
            //    return;

            //if (!NetworkServer.active)
            //    return;

            SteamMatchmaking.LeaveLobby(LobbyId);
            ResetLobbyData();
            Steamworks.StopConnection(true);

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
                Disband();
                return;
            }

            SteamMatchmaking.LeaveLobby(LobbyId);
            ResetLobbyData();
            Steamworks.StopConnection(false);

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

            Steamworks.SetClientAddress(pchValue);
            Steamworks.StartConnection(true);

            IsCreated = true;

            OnLobbyCreated?.Invoke(callback);
        }

        private void HandleJoinRequest(GameLobbyJoinRequested_t callback)
        {
            if (!SteamMatchmaking.RequestLobbyData(callback.m_steamIDLobby))
            {
                UnityEngine.Debug.Log($"Failed to connect to lobby with id \"{callback.m_steamIDLobby}\"");
                return;
            }

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

            OnJoinRequested.Invoke(callback);
        }

        private void HandleLobbyEnter(LobbyEnter_t callback)
        {
            LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            LobbyName = SteamMatchmaking.GetLobbyData(LobbyId, "name");

            //if (IsHost())
            //{
            //    OnLobbyEnter.Invoke(callback);

            //    return;
            //}

            Steamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(LobbyId, HostAddressKey));
            
            Steamworks.StartConnection(false);

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
            => NetManager.IsHostStarted;

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
