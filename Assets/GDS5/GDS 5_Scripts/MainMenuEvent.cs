using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuEvent : MonoBehaviour
{
    private const string GameplaySceneName = "Mechanic test";
    private const string StartButtonName = "StartGameButton";
    private const string QuitButtonName = "QuitGameButton";

    private Button _startButton;
    private Button _quitButton;

    private void OnEnable()
    {
        var document = GetComponent<UIDocument>();
        var root = document.rootVisualElement;

        _startButton = root.Q<Button>(StartButtonName);
        _quitButton = root.Q<Button>(QuitButtonName);

        if (_startButton != null)
        {
            _startButton.clicked += StartGame;
        }
        else
        {
            Debug.LogError($"Button '{StartButtonName}' was not found.", this);
        }

        if (_quitButton != null)
        {
            _quitButton.clicked += QuitGame;
        }
        else
        {
            Debug.LogError($"Button '{QuitButtonName}' was not found.", this);
        }
    }

    private void OnDisable()
    {
        if (_startButton != null)
        {
            _startButton.clicked -= StartGame;
        }

        if (_quitButton != null)
        {
            _quitButton.clicked -= QuitGame;
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(GameplaySceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
// Connects the main-menu buttons to the gameplay scene and application exit behavior.
