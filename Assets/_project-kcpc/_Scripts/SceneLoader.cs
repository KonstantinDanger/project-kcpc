using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectKCPC.Scripts
{
    public class SceneLoader
    {
        public void Load(string sceneName, IEnumerable<NetworkConnection> connections = null)
        {
            if (!InstanceFinder.IsServerStarted)
                return;

            SceneLoadData loadData = new(sceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };

            if (connections != null)
            {
                InstanceFinder.SceneManager.LoadConnectionScenes(connections.ToArray(), loadData);
                return;
            }

            InstanceFinder.SceneManager.LoadGlobalScenes(loadData);
        }

        public void Unload(string sceneName)
        {
            if (!InstanceFinder.IsServerStarted)
                return;

            SceneUnloadData unloadData = new(sceneName);
            InstanceFinder.SceneManager.UnloadGlobalScenes(unloadData);
        }
    }
}
