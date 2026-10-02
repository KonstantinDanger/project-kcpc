#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectKCPC.Assets.Editor
{ 
    public class TestScenesWindow : EditorWindow
    {
        private const string FolderPrefKey = "TestScenesWindow.Folder";

        private string scenesFolder = "Assets";

        private readonly List<SceneEntry> scenes = new List<SceneEntry>();

        private Vector2 scrollPosition;

        [Serializable]
        private class SceneEntry
        {
            public string guid;
            public string path;
            public KeyCode hotkey = KeyCode.None;

            public string SceneName
            {
                get
                {
                    return Path.GetFileNameWithoutExtension(path);
                }
            }
        }

        [MenuItem("Tools/Test Scenes")]
        public static void Open()
        {
            TestScenesWindow window = GetWindow<TestScenesWindow>("Test Scenes");
            window.minSize = new Vector2(500, 300);
            window.Show();
        }

        private void OnEnable()
        {
            scenesFolder = EditorPrefs.GetString(FolderPrefKey, "Assets");

            RefreshScenes();

            EditorApplication.projectChanged += RefreshScenes;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= RefreshScenes;
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUILayout.Space(8);

            DrawFolderSelector();

            EditorGUILayout.Space(10);

            DrawScenesList();

            HandleHotkeys();

            // Keep checking for keyboard input.
            Repaint();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(
                "Test Scenes",
                EditorStyles.boldLabel
            );
        }

        private void DrawFolderSelector()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Scenes Repo",
                GUILayout.Width(90)
            );

            EditorGUILayout.TextField(
                scenesFolder,
                GUILayout.ExpandWidth(true)
            );

            if (GUILayout.Button("...", GUILayout.Width(35)))
            {
                string absolutePath = EditorUtility.OpenFolderPanel(
                    "Select Scenes Folder",
                    GetAbsoluteFolderPath(),
                    ""
                );

                if (!string.IsNullOrEmpty(absolutePath))
                {
                    string projectPath = Path.GetFullPath(
                        Path.Combine(Application.dataPath, "..")
                    );

                    projectPath = projectPath.Replace("\\", "/");
                    absolutePath = absolutePath.Replace("\\", "/");

                    if (absolutePath.StartsWith(projectPath))
                    {
                        string relativePath = absolutePath.Substring(
                            projectPath.Length
                        );

                        if (relativePath.StartsWith("/"))
                            relativePath = relativePath.Substring(1);

                        scenesFolder = relativePath;

                        EditorPrefs.SetString(
                            FolderPrefKey,
                            scenesFolder
                        );

                        RefreshScenes();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(
                            "Invalid Folder",
                            "Please select a folder inside the Unity project.",
                            "OK"
                        );
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                "All scenes inside this folder and its subfolders will be listed.",
                EditorStyles.miniLabel
            );
        }

        private void DrawScenesList()
        {
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField(
                "Test Scenes:",
                EditorStyles.boldLabel
            );

            if (scenes.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No scenes were found in the selected folder.",
                    MessageType.Info
                );

                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition
            );

            for (int i = 0; i < scenes.Count; i++)
            {
                DrawSceneEntry(i, scenes[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSceneEntry(int index, SceneEntry scene)
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.helpBox
            );

            // Scene name
            EditorGUILayout.LabelField(
                scene.SceneName,
                GUILayout.MinWidth(150)
            );

            // Load button
            if (GUILayout.Button(
                    "Load",
                    GUILayout.Width(70)))
            {
                LoadScene(scene);
            }

            // Hotkey
            EditorGUI.BeginChangeCheck();

            KeyCode newHotkey = (KeyCode)EditorGUILayout.EnumPopup(
                scene.hotkey,
                GUILayout.Width(110)
            );

            if (EditorGUI.EndChangeCheck())
            {
                if (newHotkey == KeyCode.None)
                {
                    scene.hotkey = KeyCode.None;
                }
                else if (IsHotkeyAlreadyUsed(
                    newHotkey,
                    scene))
                {
                    EditorUtility.DisplayDialog(
                        "Hotkey Already Used",
                        $"The hotkey {newHotkey} is already assigned to another scene.",
                        "OK"
                    );
                }
                else
                {
                    scene.hotkey = newHotkey;
                    SaveHotkeys();
                }
            }

            EditorGUILayout.EndHorizontal();

            // Show path below the scene name.
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(8);

            EditorGUILayout.LabelField(
                scene.path,
                EditorStyles.miniLabel
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
        }

        private void LoadScene(SceneEntry scene)
        {
            if (!EditorApplication.isPlaying)
                return;

            if (string.IsNullOrEmpty(scene.path))
                return;

            SceneManager.LoadScene(scene.path);
        }

        private void HandleHotkeys()
        {
            Event currentEvent = Event.current;

            if (currentEvent == null)
                return;

            if (currentEvent.type != EventType.KeyDown)
                return;

            // Don't process modifier combinations here.
            // This keeps the hotkeys simple and avoids conflicts
            // with common Unity shortcuts.
            if (currentEvent.control ||
                currentEvent.command ||
                currentEvent.alt ||
                currentEvent.shift)
            {
                return;
            }

            KeyCode pressedKey = currentEvent.keyCode;

            if (pressedKey == KeyCode.None)
                return;

            for (int i = 0; i < scenes.Count; i++)
            {
                SceneEntry scene = scenes[i];

                if (scene.hotkey == pressedKey)
                {
                    LoadScene(scene);

                    currentEvent.Use();

                    return;
                }
            }
        }

        private string GetAbsoluteFolderPath()
        {
            if (string.IsNullOrEmpty(scenesFolder))
                return Application.dataPath;

            string projectPath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..")
            );

            return Path.GetFullPath(
                Path.Combine(projectPath, scenesFolder)
            );
        }

        private bool IsHotkeyAlreadyUsed(
            KeyCode key,
            SceneEntry ignoredScene)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                SceneEntry scene = scenes[i];

                if (scene == ignoredScene)
                    continue;

                if (scene.hotkey == key)
                    return true;
            }

            return false;
        }

        private void RefreshScenes()
        {
            scenes.Clear();

            if (string.IsNullOrEmpty(scenesFolder))
                return;

            string absoluteFolder = Path.GetFullPath(
                Path.Combine(
                    Application.dataPath,
                    "..",
                    scenesFolder
                )
            );

            if (!Directory.Exists(absoluteFolder))
                return;

            string[] files = Directory.GetFiles(
                absoluteFolder,
                "*.unity",
                SearchOption.AllDirectories
            );

            Array.Sort(files);

            for (int i = 0; i < files.Length; i++)
            {
                string absolutePath = files[i]
                    .Replace("\\", "/");

                string projectPath = Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        ".."
                    )
                ).Replace("\\", "/");

                string relativePath =
                    absolutePath.Substring(
                        projectPath.Length + 1
                    );

                string guid = AssetDatabase.AssetPathToGUID(
                    relativePath
                );

                SceneEntry entry = new SceneEntry
                {
                    guid = guid,
                    path = relativePath,
                    hotkey = LoadHotkey(guid)
                };

                scenes.Add(entry);
            }
        }

        private KeyCode LoadHotkey(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return KeyCode.None;

            string key =
                $"TestScenesWindow.Hotkey.{guid}";

            int value = EditorPrefs.GetInt(
                key,
                (int)KeyCode.None
            );

            return (KeyCode)value;
        }

        private void SaveHotkeys()
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                SceneEntry scene = scenes[i];

                if (string.IsNullOrEmpty(scene.guid))
                    continue;

                string key =
                    $"TestScenesWindow.Hotkey.{scene.guid}";

                EditorPrefs.SetInt(
                    key,
                    (int)scene.hotkey
                );
            }
        }
    }
}
#endif

