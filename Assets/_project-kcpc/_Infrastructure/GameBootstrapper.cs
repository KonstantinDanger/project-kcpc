using UnityEngine;
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
        // Не переключаем сцену, если мы запустили не BootScene или MainMenuScene
        // Это позволяет запускать любую сцену (например Gym) напрямую в редакторе.
        string currentScene = SceneManager.GetActiveScene().name;
        
        // ВРЕМЕННАЯ ПРОВЕРКА: загружаем только если мы действительно в BootScene
        if (currentScene == "BootScene")
        {
            Debug.Log($"[GameBootstrapper] Мы в BootScene, загружаем: {_startingSceneName}");
            SceneManager.LoadScene(_startingSceneName);
        }
        else
        {
            Debug.Log($"[GameBootstrapper] Мы в сцене: {currentScene}. Пропускаем загрузку.");
        }
    }
}
