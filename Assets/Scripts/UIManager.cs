using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI _scoreText, _gameOverText, _restartGameText;
    [SerializeField] private Image _livesImage;
    [SerializeField] private Sprite[] _livesSprites;
    private readonly WaitForSeconds _flickerDelay = new(0.25f);

    private void Start()
    {
        UpdateScoreText(0);
    }

    public void UpdateScoreText(int score)
    {
        _scoreText.text = "Score: " + score.ToString();
    }

    public void UpdateLivesDisplay(int currentLives)
    {
        currentLives = Mathf.Clamp(currentLives, 0, _livesSprites.Length - 1);

        _livesImage.sprite = _livesSprites[currentLives];

        if (currentLives == 0)
            ActivateGameOver();        
    }

    private void ActivateGameOver()
    {
        _gameOverText.gameObject.SetActive(true);
        _restartGameText.gameObject.SetActive(true);
        _ = StartCoroutine(GameOverFlickerRoutine());
        GameManager.Instance.GameOver();
    }

    private IEnumerator GameOverFlickerRoutine()
    {
        while (true)
        {
            _gameOverText.text = "GAME OVER";
            yield return _flickerDelay;
            _gameOverText.text = "";
            yield return _flickerDelay;
        }
    }
}
