using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class GameBootstrapper : IStartable
{
    private string _startingSceneName;

    [Inject]
    public GameBootstrapper(StaticData staticData)
    {
        _startingSceneName = staticData.StartingSceneName;
    }

    public void Start()
    {
        SceneManager.LoadScene(_startingSceneName);
    }
}
