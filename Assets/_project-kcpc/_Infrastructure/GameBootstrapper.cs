using UnityEngine.SceneManagement;
using VContainer.Unity;

public class GameBootstrapper : IStartable
{
    public void Start()
    {
        SceneManager.LoadScene(1);

    }
}
