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
                    Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                    
                    if (map[x, y] == 1)
                    {
                        if (wallPrefab != null)
                            Instantiate(wallPrefab, position + Vector3.up * (cellSize / 2), Quaternion.identity, transform);
                    }
                    else
                    {
                        if (floorPrefab != null)
                            Instantiate(floorPrefab, position, Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}
