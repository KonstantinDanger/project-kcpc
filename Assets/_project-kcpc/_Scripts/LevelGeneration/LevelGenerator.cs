using UnityEngine;

namespace ProjectKCPC.LevelGeneration
{
    public class LevelGenerator : MonoBehaviour
    {
        /// <summary>
        /// Выравнивает комнату (roomToMove) так, чтобы её сокет (socketToConnect) 
        /// идеально совпал с сокетом (targetSocket) уже размещенной комнаты, 
        /// а их направления были строго противоположны.
        /// </summary>
        /// <param name="roomToMove">Комната, которую мы хотим разместить</param>
        /// <param name="socketToConnect">Сокет на комнате, которую мы размещаем</param>
        /// <param name="targetSocket">Сокет на уже размещенной комнате, к которому мы стыкуемся</param>
        public static void AlignRoom(RoomModule roomToMove, RoomSocket socketToConnect, RoomSocket targetSocket)
        {
            // ШАГ 1: ВРАЩЕНИЕ
            // Нам нужно повернуть roomToMove так, чтобы её сокет (socketToConnect) смотрел 
            // прямо в противоположную сторону от целевого сокета (targetSocket).
            
            // Получаем вектор направления, в котором ДОЛЖЕН смотреть наш сокет после вращения.
            // Это направление, противоположное направлению целевого сокета (-targetSocket.Forward).
            Vector3 targetForward = -targetSocket.Forward;

            // Находим кватернион (вращение), который нужен, чтобы повернуть 
            // текущее направление нашего сокета к желаемому направлению (targetForward).
            Quaternion rotationOffset = Quaternion.FromToRotation(socketToConnect.Forward, targetForward);

            // Применяем это вращение ко всей комнате. 
            // Это повернет комнату вокруг её пивота (transform.position).
            // ВАЖНО: В Unity применение FromToRotation к текущему вращению выглядит так:
            // НовоеВращение = ВращениеОффсета * ТекущееВращение
            roomToMove.transform.rotation = rotationOffset * roomToMove.transform.rotation;

            // ШАГ 2: ПОЗИЦИОНИРОВАНИЕ
            // Теперь, когда комната правильно повернута, нам нужно её сдвинуть.
            // Внимание: после вращения на Шаге 1, позиция socketToConnect в пространстве изменилась,
            // потому что комната вращалась вокруг своего пивота, а сокет находится где-то сбоку.

            // Вычисляем вектор смещения от текущей позиции нашего сокета до целевого сокета.
            // Формула: ВекторОтАдоБ = ТочкаБ - ТочкаА
            Vector3 positionOffset = targetSocket.transform.position - socketToConnect.transform.position;

            // Прибавляем этот вектор смещения к позиции всей комнаты.
            // Поскольку сокет является дочерним объектом комнаты, он сдвинется ровно на такой же вектор
            // и окажется точно в точке targetSocket.transform.position.
            roomToMove.transform.position += positionOffset;
            
            // Итог: комната теперь стоит так, что её сокет совпадает с целевым и по позиции, и по направлению (но инвертированно).
        }

        /// <summary>
        /// Проверяет, пересекается ли габаритный контейнер (BoxCollider) проверяемой комнаты 
        /// с другими объектами на сцене (другими комнатами).
        /// </summary>
        /// <param name="roomToCheck">Комната, которую мы хотим проверить на пересечения (должна быть уже позиционирована)</param>
        /// <param name="roomLayerMask">Маска слоев, на которых находятся комнаты (опционально, для оптимизации)</param>
        /// <returns>True - если место ЗАБЛОКИРОВАНО (есть пересечение). False - если место СВОБОДНО.</returns>
        public static bool CheckCollision(RoomModule roomToCheck, LayerMask? roomLayerMask = null)
        {
            // Получаем мировые габариты комнаты из её коллайдера
            Bounds bounds = roomToCheck.GetBoundingBox();

            // Physics.OverlapBox требует центр, половину размера (extents) и поворот.
            // BoxCollider уже вычисляет мировые Center и Extents для Bounds (без учета поворота коробки, Bounds - это AABB, т.е. коробка выровненная по осям мира).
            // НО! Если комната повернута, её AABB (Bounds) будет больше, чем сама комната, чтобы вместить её при повороте.
            // Это может привести к "ложным" коллизиям на углах.
            // Более точный метод - использовать локальные данные BoxCollider и передавать поворот комнаты.

            // Попробуем получить коллайдер напрямую для более точной OBB (Oriented Bounding Box) проверки
            BoxCollider boxColl = roomToCheck.GetComponent<BoxCollider>();
            
            Vector3 center;
            Vector3 halfExtents;
            Quaternion orientation;

            if (boxColl != null)
            {
                // Если есть BoxCollider, делаем точную проверку с учетом вращения комнаты
                // TransformPoint переводит локальный центр коллайдера в мировые координаты с учетом масштаба, позиции и поворота комнаты
                center = roomToCheck.transform.TransformPoint(boxColl.center);
                
                // Extents - это половина размера. Мы умножаем локальный размер коллайдера на глобальный масштаб комнаты
                // и делим на 2, чтобы получить половину (радиус по осям)
                halfExtents = Vector3.Scale(boxColl.size, roomToCheck.transform.lossyScale) * 0.5f;
                
                // Вращение самой коробки совпадает с вращением комнаты
                orientation = roomToCheck.transform.rotation;
            }
            else
            {
                // Фолбэк на AABB, если нет BoxCollider (менее точно)
                center = bounds.center;
                halfExtents = bounds.extents;
                orientation = Quaternion.identity;
            }

            // Немного уменьшаем halfExtents (на миллиметры), чтобы избежать ложных срабатываний 
            // при идеальном прилегании стен двух соседних комнат (когда они касаются, но не пересекаются)
            halfExtents -= Vector3.one * 0.05f;

            // Временный массив для хранения результатов OverlapBox (чтобы не аллоцировать память каждый раз)
            Collider[] hitColliders = new Collider[10];
            
            // Если маска не передана, проверяем все слои по умолчанию (Default)
            int mask = roomLayerMask.HasValue ? roomLayerMask.Value.value : Physics.DefaultRaycastLayers;

            // Выполняем проверку коллизий. NonAlloc версия более производительна для частых вызовов.
            int numColliders = Physics.OverlapBoxNonAlloc(center, halfExtents, hitColliders, orientation, mask);

            for (int i = 0; i < numColliders; i++)
            {
                Collider hit = hitColliders[i];
                
                // Игнорируем коллайдеры самой проверяемой комнаты и её дочерних объектов
                if (hit.transform.IsChildOf(roomToCheck.transform))
                {
                    continue;
                }

                // Игнорируем триггеры, если это необходимо (например, зоны появления мобов)
                // Если ваши комнаты используют коллайдеры-триггеры для определения размеров, закомментируйте эту проверку.
                if (hit.isTrigger)
                {
                    continue;
                }

                // Если мы дошли сюда, значит мы столкнулись с ЧУЖИМ физическим объектом. Место занято.
                return true; 
            }

            // Пересечений не найдено (или все они были проигнорированы). Место свободно.
            return false;
        }
    }
}
