#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectKCPC.Assets.Editor
{
    [InitializeOnLoad]
    public class ForceStartupScene
    {
        private const string previousScenePathKey = "PreviousSceneKey";

        private static string bootScenePath = "Assets/_project-kcpc/_Scenes/BootScene.unity";

        static ForceStartupScene()
            => EditorApplication.playModeStateChanged += HandlePlayModeChanged;

        private static void HandlePlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                string currentScene = EditorSceneManager.GetActiveScene().path;
                EditorPrefs.SetString(previousScenePathKey, currentScene);

                if (currentScene != bootScenePath)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(bootScenePath);
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                string previousScenePath = EditorPrefs.GetString(previousScenePathKey);

                if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != bootScenePath)
                {
                    EditorSceneManager.OpenScene(previousScenePath);
                }
            }
        }
    }
}
#endif