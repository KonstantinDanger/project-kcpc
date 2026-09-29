using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectKCPC
{
    public class Gym : IStartable
    {
        private StaticData _staticData;
        private SpawnPoint _spawnPoint;

        [Inject]
        public Gym(StaticData staticData, SpawnPoint spawnPoint)
        {
            _staticData = staticData;
            _spawnPoint = spawnPoint;
        }

        public void Start()
        {
            GameObject.Instantiate(_staticData.PlayerPrefab, _spawnPoint.transform.position, _spawnPoint.transform.rotation);
        }
    }
}
