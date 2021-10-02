using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The universal game manager.
/// </summary>
public class Game : MonoBehaviour
{
    private enum SceneName { CheckersScene, ChessScene, MainMenuScene, FroggerScene, PongScene }

    public void GoToChessScene()
    {
        SceneManager.LoadScene(SceneName.ChessScene.ToString());
    }

    public void GoToPongScene()
    {
        SceneManager.LoadScene(SceneName.PongScene.ToString());
    }
}
