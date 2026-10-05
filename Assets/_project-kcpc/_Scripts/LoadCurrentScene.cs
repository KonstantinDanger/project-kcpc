using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadCurrentScene : MonoBehaviour
{
    void Start()
    {
        // Получаем объект текущей активной сцены
        Scene currentScene = SceneManager.GetActiveScene();
        
        // Узнаем имя сцены
        string sceneName = currentScene.name;
        
        Debug.Log("Текущая сцена: " + sceneName);
    }

    // Пример метода для перезагрузки текущей сцены
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}