using System.Collections.Generic;
using UnityEngine;

namespace ProjectKCPC.LevelGeneration
{
    [RequireComponent(typeof(BoxCollider))]
    public class RoomModule : MonoBehaviour
    {
        [Tooltip("Список всех доступных точек стыковки (сокетов) в этой комнате")]
        public List<RoomSocket> Sockets = new List<RoomSocket>();

        [Tooltip("Коллайдер, используемый для определения габаритов комнаты. Если не задан, возьмет компонент с этого объекта.")]
        [SerializeField] private BoxCollider boundsCollider;

        private void Awake()
        {
            if (boundsCollider == null)
            {
                boundsCollider = GetComponent<BoxCollider>();
            }
        }

        /// <summary>
        /// Возвращает мировые габариты (Bounds) комнаты на основе BoxCollider.
        /// </summary>
        public Bounds GetBoundingBox()
        {
            if (boundsCollider != null)
            {
                return boundsCollider.bounds;
            }

            Debug.LogWarning($"[RoomModule] На объекте {gameObject.name} не найден BoxCollider для получения габаритов!", this);
            return new Bounds(transform.position, Vector3.zero);
        }

        /// <summary>
        /// Удобная кнопка в инспекторе для автоматического сбора всех сокетов в дочерних объектах
        /// </summary>
        [ContextMenu("Find All Sockets")]
        private void FindSockets()
        {
            Sockets = new List<RoomSocket>(GetComponentsInChildren<RoomSocket>());
            foreach (var socket in Sockets)
            {
                socket.ParentRoom = this;
            }
            
            if (boundsCollider == null)
            {
                boundsCollider = GetComponent<BoxCollider>();
            }
            
            Debug.Log($"[RoomModule] Найдено {Sockets.Count} сокетов.");
        }
    }
}
