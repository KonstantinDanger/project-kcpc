using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace KCPC.LevelGeneration
{
    public class MazeGenerator : NetworkBehaviour
    {
        [Header("Maze Settings")]
        public int width = 10;
        public int height = 10;
        public float cellSize = 5f;
        
        [Header("Prefabs")]
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        // Optionally, these could be NetworkObjects if they need networked state (like breakable walls),
        // but for static geometry, regular GameObjects are fine as long as generation is deterministic.
        
        private readonly SyncVar<int> mazeSeed = new SyncVar<int>();

        private void Awake()
        {
            mazeSeed.OnChange += OnSeedChanged;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Server generates a random seed and syncs it to clients
            mazeSeed.Value = Random.Range(1000, 999999);
            GenerateMazeLocally(mazeSeed.Value);
        }

        private void OnSeedChanged(int oldSeed, int newSeed, bool asServer)
        {
            if (!asServer)
            {
                // Clients generate the exact same maze using the synced seed
                GenerateMazeLocally(newSeed);
            }
        }

        [ContextMenu("Generate Maze Locally")]
        public void GenerateMaze() 
        {
            mazeSeed.Value = Random.Range(1000, 999999);
            // INSERT_YOUR_CODE
            // Destroy all existing maze child objects to clear previous generation
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
 
            GenerateMazeLocally(mazeSeed.Value);

        }

        private void GenerateMazeLocally(int seed)
        {
            // Set the random seed so generation is identical on server and all clients
            Random.InitState(seed);

            // Simple Random Walk / Cellular Automata or Maze Logic can go here.
            // For this example, we generate a basic grid map where 1 is a wall and 0 is floor.
            int[,] map = new int[width, height];

            // Fill with walls initially
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = 1;
                }
            }

            // Simple Drunkard's Walk for Backrooms-like corridors
            int currentX = width / 2;
            int currentY = height / 2;
            map[currentX, currentY] = 0;

            int maxSteps = (width * height) / 2;
            for (int i = 0; i < maxSteps; i++)
            {
                int dir = Random.Range(0, 4);
                switch (dir)
                {
                    case 0: currentY++; break; // Up
                    case 1: currentY--; break; // Down
                    case 2: currentX--; break; // Left
                    case 3: currentX++; break; // Right
                }

                // Clamp to bounds (leave outer edges as walls)
                currentX = Mathf.Clamp(currentX, 1, width - 2);
                currentY = Mathf.Clamp(currentY, 1, height - 2);

                map[currentX, currentY] = 0; // Dig out floor
            }

            // Build the geometry
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 localPos = new Vector3(x * cellSize, 0, y * cellSize);
                    
                    // We only build geometry for floor cells (0) and place walls on their edges
                    if (map[x, y] == 0)
                    {
                        if (floorPrefab != null)
                        {
                            GameObject floor = Instantiate(floorPrefab, transform);
                            floor.transform.localPosition = localPos;
                            floor.transform.localRotation = Quaternion.identity;
                        }

                        if (wallPrefab != null)
                        {
                            // Forward (Up)
                            if (y == height - 1 || map[x, y + 1] == 1)
                            {
                                GameObject wall = Instantiate(wallPrefab, transform);
                                wall.transform.localPosition = localPos + new Vector3(0, cellSize / 2, cellSize / 2);
                                wall.transform.localRotation = Quaternion.Euler(0, 0, 0);
                            }
                            // Backward (Down)
                            if (y == 0 || map[x, y - 1] == 1)
                            {
                                GameObject wall = Instantiate(wallPrefab, transform);
                                wall.transform.localPosition = localPos + new Vector3(0, cellSize / 2, -cellSize / 2);
                                wall.transform.localRotation = Quaternion.Euler(0, 180, 0);
                            }
                            // Right
                            if (x == width - 1 || map[x + 1, y] == 1)
                            {
                                GameObject wall = Instantiate(wallPrefab, transform);
                                wall.transform.localPosition = localPos + new Vector3(cellSize / 2, cellSize / 2, 0);
                                wall.transform.localRotation = Quaternion.Euler(0, 90, 0);
                            }
                            // Left
                            if (x == 0 || map[x - 1, y] == 1)
                            {
                                GameObject wall = Instantiate(wallPrefab, transform);
                                wall.transform.localPosition = localPos + new Vector3(-cellSize / 2, cellSize / 2, 0);
                                wall.transform.localRotation = Quaternion.Euler(0, -90, 0);
                            }
                        }
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
namespace KCPC.LevelGeneration
{
    [UnityEditor.CustomEditor(typeof(MazeGenerator))]
    public class MazeGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MazeGenerator generator = (MazeGenerator)target;
            if (UnityEngine.GUILayout.Button("Generate Maze"))
            {
                generator.GenerateMaze();
            }
        }
    }
}
#endif
