using UnityEngine;
using VContainer.Unity;

namespace ProjectKCPC
{
    public abstract class BaseLifeTimeScope : LifetimeScope 
    {
        [SerializeField] private SpawnPoint _spawnPoint;

        public SpawnPoint SpawnPoint => _spawnPoint; 
    }
}
