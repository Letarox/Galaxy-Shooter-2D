using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    private bool _isGameOver = false;
    private bool _playerWon = false;

    public bool IsGameOver => _isGameOver;
    public bool PlayerWon => _playerWon;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && (_isGameOver || _playerWon))
        {
            SceneManager.LoadScene(1);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
    }

    public void Victory()
    {
        _playerWon = true;
        UIManager.Instance.ActivateVictory();
    }
}
