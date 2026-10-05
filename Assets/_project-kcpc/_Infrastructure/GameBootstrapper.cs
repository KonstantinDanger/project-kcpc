using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class GameBootstrapper : IStartable
{
    private readonly string _startingSceneName;

    [Inject]
    public GameBootstrapper(StaticData staticData)
    {
        _startingSceneName = staticData.MainMenuScene;
    }

    public void Start()
    {
        SceneManager.LoadScene(_startingSceneName);
    }
}
