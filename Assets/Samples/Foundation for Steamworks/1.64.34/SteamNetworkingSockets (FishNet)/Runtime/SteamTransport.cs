// Copyright 2015-2026 Heathen Engineering
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if !DISABLESTEAMWORKS && FOUNDATION && STEAM_INSTALLED && FISHNET_INSTALLED
using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Transporting;
using Heathen.SteamworksIntegration;
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration.FishNetTransport
{
    /// <summary>
    /// FishNet <see cref="Transport"/> implementation over Steam Networking Sockets, built on
    /// <see cref="Heathen.SteamworksIntegration.SteamNetworkingSocketsHelper"/> from the core
    /// Steamworks Foundation package. Always addresses by Steam identity, never by raw IP -- that
    /// is what "using Steam Networking Sockets properly" means; there is deliberately no IP-address
    /// mode here to toggle.
    ///
    /// <para>Three roles, not two: a <b>Client</b> always uses the client interface, full stop -- see
    /// <see cref="StartConnection"/>, which never consults <see cref="steamGameListenServer"/>. A
    /// <b>Dedicated Server</b> (no client in the process at all) always uses the Game Server interface.
    /// A <b>Listen Server</b> (both in one process) can validly use either -- that choice is
    /// <see cref="steamGameListenServer"/>, and it is never inferred from whether an SGS happens to be
    /// initialised for some other reason; the developer states it explicitly.</para>
    ///
    /// <para>Steam's connection handles are unsigned and opaque, so the transport hands FishNet its own
    /// small sequential connection ids and keeps the mapping. Received messages are tagged
    /// <see cref="Channel.Reliable"/> because the helper does not report a message's reliability; data
    /// is still sent on the channel FishNet asked for.</para>
    /// </summary>
    public class SteamTransport : Transport
    {
        private const int SteamMtu = 1200;

        [Tooltip("Listen Server will use Steam Game Server APIs when this is true, else Listen Server will use client API when this is false.")]
        [SerializeField] private bool _steamGameListenServer;

        [Header("Client Settings")]
        [Tooltip("The Steam ID of the host to connect to.")]
        [SerializeField] private string _address = "";

        [Header("Server Settings")]
        [Tooltip("The most remote clients the server accepts at once.")]
        [SerializeField] private int _maximumClients = 4095;

        /// <summary>Whether the server side of this transport should use the Steam Game Server networking interface. See the class remarks for the three-role model this decides between.</summary>
        public bool steamGameListenServer { get => _steamGameListenServer; set => _steamGameListenServer = value; }
        /// <summary>The Steam ID of the host to connect to.</summary>
        public string address { get => _address; set => _address = value; }

        private LocalConnectionState _serverState = LocalConnectionState.Stopped;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;

        private SteamNetworkingSocketsListener _listener;
        private SteamNetworkingSocketsConnector _connector;
        private Coroutine _connectRoutine;

        private readonly Dictionary<int, int> _fishToSteam = new();
        private readonly Dictionary<int, int> _steamToFish = new();
        private int _nextConnectionId;

        /// <inheritdoc/>
        public override event Action<ClientConnectionStateArgs> OnClientConnectionState;
        /// <inheritdoc/>
        public override event Action<ServerConnectionStateArgs> OnServerConnectionState;
        /// <inheritdoc/>
        public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;
        /// <inheritdoc/>
        public override event Action<ClientReceivedDataArgs> OnClientReceivedData;
        /// <inheritdoc/>
        public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

        private void OnDestroy() => Shutdown();

        /// <inheritdoc/>
        public override string GetConnectionAddress(int connectionId) => _fishToSteam.ContainsKey(connectionId) ? $"steam:{connectionId}" : string.Empty;

        /// <inheritdoc/>
        public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs) => OnClientConnectionState?.Invoke(connectionStateArgs);
        /// <inheritdoc/>
        public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs) => OnServerConnectionState?.Invoke(connectionStateArgs);
        /// <inheritdoc/>
        public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs) => OnRemoteConnectionState?.Invoke(connectionStateArgs);
        /// <inheritdoc/>
        public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs) => OnClientReceivedData?.Invoke(receivedDataArgs);
        /// <inheritdoc/>
        public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs) => OnServerReceivedData?.Invoke(receivedDataArgs);

        /// <inheritdoc/>
        public override LocalConnectionState GetConnectionState(bool server) => server ? _serverState : _clientState;

        /// <inheritdoc/>
        public override RemoteConnectionState GetConnectionState(int connectionId) =>
            _fishToSteam.ContainsKey(connectionId) ? RemoteConnectionState.Started : RemoteConnectionState.Stopped;

        private void SetServerState(LocalConnectionState state)
        {
            if (_serverState == state) return;
            _serverState = state;
            HandleServerConnectionState(new ServerConnectionStateArgs(state, Index));
        }

        private void SetClientState(LocalConnectionState state)
        {
            if (_clientState == state) return;
            _clientState = state;
            HandleClientConnectionState(new ClientConnectionStateArgs(state, Index));
        }

        /// <inheritdoc/>
        public override bool StartConnection(bool server) => server ? StartServer() : StartClient();

        /// <inheritdoc/>
        public override bool StopConnection(bool server) => server ? StopServer() : StopClient();

        /// <inheritdoc/>
        public override bool StopConnection(int connectionId, bool immediately)
        {
            if (!_fishToSteam.TryGetValue(connectionId, out var steamConnection) || _listener == null)
                return false;

            _listener.Kick(steamConnection);
            return true;
        }

        /// <inheritdoc/>
        public override void Shutdown()
        {
            StopClient();
            StopServer();
        }

        private bool StartServer()
        {
            if (_serverState.IsStartedOrStarting())
                return false;

            SetServerState(LocalConnectionState.Starting);

            _listener = new SteamNetworkingSocketsListener(_steamGameListenServer);
            _listener.onConnectionAccepted += HandleConnectionAccepted;
            _listener.onConnectionClosed += HandleConnectionClosed;
            _listener.onMessageReceived += HandleServerMessage;

            if (!_listener.ListenByIdentity())
            {
                _listener = null;
                SetServerState(LocalConnectionState.Stopped);
                return false;
            }

            SetServerState(LocalConnectionState.Started);
            return true;
        }

        private bool StopServer()
        {
            if (_serverState.IsStoppedOrStopping())
                return false;

            SetServerState(LocalConnectionState.Stopping);

            foreach (var fishId in new List<int>(_fishToSteam.Keys))
                HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, fishId, Index));

            _listener?.StopListening();
            _listener = null;
            _fishToSteam.Clear();
            _steamToFish.Clear();
            _nextConnectionId = 0;

            SetServerState(LocalConnectionState.Stopped);
            return true;
        }

        private void HandleConnectionAccepted(int steamConnection)
        {
            if (_fishToSteam.Count >= _maximumClients)
            {
                _listener?.Kick(steamConnection);
                return;
            }

            var fishId = _nextConnectionId++;
            _fishToSteam[fishId] = steamConnection;
            _steamToFish[steamConnection] = fishId;
            HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Started, fishId, Index));
        }

        private void HandleConnectionClosed(int steamConnection)
        {
            if (!_steamToFish.TryGetValue(steamConnection, out var fishId))
                return;

            HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, fishId, Index));
            _steamToFish.Remove(steamConnection);
            _fishToSteam.Remove(fishId);
        }

        private void HandleServerMessage(int steamConnection, byte[] buffer, int offset, int length)
        {
            if (!_steamToFish.TryGetValue(steamConnection, out var fishId))
                return;

            HandleServerReceivedDataArgs(new ServerReceivedDataArgs(new ArraySegment<byte>(buffer, offset, length), _listener != null && !_listener.lastMessageReliable ? Channel.Unreliable : Channel.Reliable, fishId, Index));
        }

        private bool StartClient()
        {
            if (_clientState.IsStartedOrStarting())
                return false;

            CloseConnector();
            _connector = new SteamNetworkingSocketsConnector();
            _connector.onConnected += () => SetClientState(LocalConnectionState.Started);
            _connector.onDisconnected += () => SetClientState(LocalConnectionState.Stopped);
            _connector.onMessageReceived += (buffer, offset, length) =>
                HandleClientReceivedDataArgs(new ClientReceivedDataArgs(new ArraySegment<byte>(buffer, offset, length), _connector != null && !_connector.lastMessageReliable ? Channel.Unreliable : Channel.Reliable, Index));

            SetClientState(LocalConnectionState.Starting);
            _connectRoutine = StartCoroutine(ConnectRoutine(_address));
            return true;
        }

        private IEnumerator ConnectRoutine(string steamId)
        {
            yield return null;

            if (!ulong.TryParse(steamId, out var id))
            {
                HeathenDebug.LogError(typeof(SteamworksSubsystem), $"[{nameof(SteamTransport)}] Invalid Steam ID address: {steamId}");
                CloseConnector();
                SetClientState(LocalConnectionState.Stopped);
                yield break;
            }

            _connector.ConnectByIdentity(new CSteamID(id));
        }

        private bool StopClient()
        {
            if (_clientState.IsStoppedOrStopping())
            {
                CloseConnector();
                return false;
            }

            SetClientState(LocalConnectionState.Stopping);
            CloseConnector();
            SetClientState(LocalConnectionState.Stopped);
            return true;
        }

        private void CloseConnector()
        {
            if (_connectRoutine != null) { StopCoroutine(_connectRoutine); _connectRoutine = null; }
            _connector?.Disconnect();
            _connector = null;
        }

        /// <inheritdoc/>
        public override void SendToServer(byte channelId, ArraySegment<byte> segment) =>
            _connector?.Send(segment.Array, segment.Offset, segment.Count, ToReliability(channelId));

        /// <inheritdoc/>
        public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
        {
            if (_listener != null && _fishToSteam.TryGetValue(connectionId, out var steamConnection))
                _listener.Send(steamConnection, segment.Array, segment.Offset, segment.Count, ToReliability(channelId));
        }

        /// <inheritdoc/>
        public override void IterateIncoming(bool asServer)
        {
            if (asServer)
            {
                if (ServerInterfaceAvailable) _listener?.ReceiveMessages();
            }
            else
            {
                _connector?.ReceiveMessages();
            }
        }

        /// <inheritdoc/>
        public override void IterateOutgoing(bool asServer)
        {
            if (asServer)
            {
                if (ServerInterfaceAvailable) _listener?.FlushMessages();
            }
            else
            {
                _connector?.FlushMessages();
            }
        }

        /// <inheritdoc/>
        public override int GetMaximumClients() => _maximumClients;

        /// <inheritdoc/>
        public override void SetMaximumClients(int value) => _maximumClients = Mathf.Max(1, value);

        /// <inheritdoc/>
        public override void SetClientAddress(string address) => _address = address;

        /// <inheritdoc/>
        public override string GetClientAddress() => _address;

        /// <inheritdoc/>
        public override int GetMTU(byte channel) => SteamMtu;

        private bool ServerInterfaceAvailable => !_steamGameListenServer || API.App.Server.Initialised;

        private static SteamSendReliability ToReliability(byte channelId) =>
            channelId == (byte)Channel.Unreliable ? SteamSendReliability.Unreliable : SteamSendReliability.Reliable;
    }
}
#endif
