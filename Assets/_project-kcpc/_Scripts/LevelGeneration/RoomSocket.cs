using UnityEngine;

namespace ProjectKCPC.LevelGeneration
{
    public class RoomSocket : MonoBehaviour
    {
        [Tooltip("Модуль комнаты, к которому принадлежит этот сокет")]
        public RoomModule ParentRoom;

        /// <summary>
        /// Направление, в котором сокет готов стыковаться с другим сокетом.
        /// Зависит от поворота объекта в пространстве.
        /// </summary>
        public Vector3 Forward => transform.forward;

        private void OnDrawGizmos()
        {
            // Отрисовка сокета для удобства в редакторе (голубая сфера и красная линия направления)
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.3f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
    }
}
