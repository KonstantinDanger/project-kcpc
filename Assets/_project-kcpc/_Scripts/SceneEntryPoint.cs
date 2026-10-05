using FishNet.Connection;
using FishNet.Managing;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectKCPC
{
    public class SceneEntryPoint : IInitializable
    {
        private readonly StaticData _staticData;
        private readonly SpawnPoint _spawnPoint;
        private readonly NetworkManager _netManager;

        [Inject]
        public SceneEntryPoint(StaticData staticData, SpawnPoint spawnPoint, NetworkManager netManager)
        {
            _staticData = staticData;
            _spawnPoint = spawnPoint;
            _netManager = netManager;
        }

        public void Initialize()
        {
            if (!_netManager.IsServerStarted)
                return;

            foreach (var clientConn in _netManager.ServerManager.Clients.Values)
                SetupPlayer(clientConn);
        }

        private void SetupPlayer(NetworkConnection conn)
        {
            if (conn.FirstObject != null)
            {
                PositionPlayer(conn.FirstObject.gameObject);
            }
            else
            {
                conn.OnObjectAdded += (np) =>
                {
                    PositionPlayer(np.gameObject);
                };
            }
        }

        private void PositionPlayer(GameObject playerObject)
        {
            playerObject.transform.SetPositionAndRotation(
                _spawnPoint.transform.position,
                _spawnPoint.transform.rotation);
        }
    }
}
