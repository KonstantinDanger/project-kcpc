using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectKCPC
{
    public class GymLifeTimeScope : LifetimeScope
    {
        [SerializeField] private SpawnPoint _spawnPoint;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_spawnPoint);
            builder.RegisterEntryPoint<Gym>(Lifetime.Scoped);
        }
    }
}
