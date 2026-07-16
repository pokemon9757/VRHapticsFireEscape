using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    [SerializeField]
    private string sceneToLoad;

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("Scene name is not set. Please specify a scene to load.");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}