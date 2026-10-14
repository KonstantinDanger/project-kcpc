using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectKCPC.LevelGeneration
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("Префаб стартовой комнаты")]
        public RoomModule startRoomPrefab;
        
        [Tooltip("Список доступных префабов комнат для генерации")]
        public RoomModule[] roomPrefabs;
        
        [Tooltip("Префаб стены/заглушки для закрытия тупиков")]
        public GameObject wallPlugPrefab;

        [Header("Settings")]
        [Tooltip("Максимальное количество комнат (не считая стартовую)")]
        public int maxRooms = 20;

        // Очередь сокетов, к которым мы еще можем пристыковать новые комнаты
        private Queue<RoomSocket> openSockets = new Queue<RoomSocket>();
        
        // Счетчик успешно заспавненных комнат
        private int spawnedRooms = 0;

        private void Start()
        {
            // Автоматический старт генерации при запуске игры
            GenerateDungeon();
        }

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            // Очищаем состояние перед новой генерацией
            openSockets.Clear();
            spawnedRooms = 0;

            if (startRoomPrefab == null || roomPrefabs.Length == 0)
            {
                Debug.LogError("[DungeonGenerator] Не назначены префабы комнат!");
                return;
            }

            // 1. Спавним StartRoom
            RoomModule startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
            
            // 2. Добавляем ее неиспользованные сокеты в openSockets
            foreach (var socket in startRoom.Sockets)
            {
                openSockets.Enqueue(socket);
            }

            // 3. Запускаем основной цикл генерации
            while (openSockets.Count > 0 && spawnedRooms < maxRooms)
            {
                // Достаем целевой сокет из очереди
                RoomSocket targetSocket = openSockets.Dequeue();

                // Выбираем случайный префаб комнаты
                RoomModule prefabToSpawn = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
                
                // Спавним новую комнату
                RoomModule newRoom = Instantiate(prefabToSpawn);

                // Выбираем случайный сокет у новой комнаты, которым она будет стыковаться
                RoomSocket socketToConnect = newRoom.Sockets[Random.Range(0, newRoom.Sockets.Count)];

                // 4. Применяем выравнивание
                LevelGenerator.AlignRoom(newRoom, socketToConnect, targetSocket);

                // Синхронизируем физику, чтобы коллайдеры новой комнаты обновили свою позицию в физическом движке
                // Это важно для корректной работы OverlapBox в CheckCollision
                Physics.SyncTransforms();

                // 5. Проверяем коллизии
                if (LevelGenerator.CheckCollision(newRoom))
                {
                    // Коллизия есть — место занято.
                    // Удаляем заспавненную комнату (используем DestroyImmediate для корректной работы в Editor скриптах/ContextMenu, 
                    // для чистого Runtime можно просто Destroy)
                    DestroyImmediate(newRoom.gameObject);
                    
                    // Ставим на этот сокет заглушку (глухую стену)
                    SpawnWallPlug(targetSocket);
                }
                else
                {
                    // Коллизии нет — фиксируем комнату
                    spawnedRooms++;

                    // Добавляем все ее свободные (оставшиеся) сокеты в openSockets
                    foreach (var socket in newRoom.Sockets)
                    {
                        // Не добавляем тот сокет, которым мы только что пристыковались!
                        if (socket != socketToConnect)
                        {
                            openSockets.Enqueue(socket);
                        }
                    }
                }
            }

            // 6. Ограничение по количеству комнат достигнуто (или закончились сокеты).
            // Закрываем все оставшиеся открытые сокеты стенами, чтобы не было дыр в пустоту.
            while (openSockets.Count > 0)
            {
                RoomSocket remainingSocket = openSockets.Dequeue();
                SpawnWallPlug(remainingSocket);
            }

            Debug.Log($"[DungeonGenerator] Генерация завершена. Создано комнат: {spawnedRooms} из {maxRooms}.");
        }

        /// <summary>
        /// Спавнит стену-заглушку в месте открытого сокета
        /// </summary>
        private void SpawnWallPlug(RoomSocket targetSocket)
        {
            if (wallPlugPrefab == null) return;

            // Спавним стену в позиции сокета
            // Поворачиваем стену так, чтобы она "смотрела" туда же, куда и сокет
            Instantiate(wallPlugPrefab, targetSocket.transform.position, targetSocket.transform.rotation);
        }
    }
}
