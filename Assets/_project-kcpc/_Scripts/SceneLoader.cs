using FishNet;
using FishNet.Managing.Scened;

namespace ProjectKCPC.Scripts
{
    public class SceneLoader
    {
        public void Load(string sceneName)
        {
            if (!InstanceFinder.IsServerStarted)
                return;

            SceneLoadData loadData = new(sceneName)
            {
                ReplaceScenes = ReplaceOption.All
            };
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
