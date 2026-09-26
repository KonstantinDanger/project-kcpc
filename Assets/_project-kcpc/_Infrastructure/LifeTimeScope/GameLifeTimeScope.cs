using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifeTimeScope : LifetimeScope
{
    [SerializeField] private StaticData _staticData;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);

        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<GameBootstrapper>(Lifetime.Singleton);
        builder.RegisterInstance(_staticData);
        Debug.Log("Bootstrapping");
    }
}
