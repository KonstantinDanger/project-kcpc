using FishNet.Managing;
using ProjectKCPC.Scripts.Lobby;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifeTimeScope : LifetimeScope
{
    [SerializeField] private StaticData _staticData;
    [SerializeField] private NetworkManager _netManager;
    [SerializeField] private FishySteamworks.FishySteamworks _steamworks;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);

        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(_netManager);
        builder.RegisterComponent(_steamworks);
        builder.RegisterInstance(_staticData);

        builder.RegisterEntryPoint<GameBootstrapper>(Lifetime.Singleton);
        builder.RegisterEntryPoint<Lobby>(Lifetime.Singleton);
    }
}
