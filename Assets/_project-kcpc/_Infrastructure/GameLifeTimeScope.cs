
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class GameLifeTimeScope : LifetimeScope
{
    [SerializeField] private StaticData _staticData;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_staticData);
        Debug.Log("Bootstrapping");
        SceneManager.LoadScene(1);
    }
}
