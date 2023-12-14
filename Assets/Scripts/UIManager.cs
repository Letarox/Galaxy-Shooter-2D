using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI _scoreText, _gameOverText, _restartGameText, _ammoText;
    [SerializeField] private Image _livesImage;
    [SerializeField] private Sprite[] _livesSprites;
    private readonly WaitForSeconds _flickerDelay = new(0.25f);

    private void Start()
    {
        UpdateScoreText(0);
        UpdateAmmoAmount(15);
    }

    public void UpdateScoreText(int score)
    {
        _scoreText.text = "Score: " + score.ToString();
    }

    public void UpdateAmmoAmount(int ammo)
    {
        _ammoText.text = "Ammo: " + ammo.ToString();

        if (ammo == 0)
            _ammoText.color = Color.red;
        else
            _ammoText.color = Color.white;
    }

    public void UpdateLivesDisplay(int currentLives)
    {
        currentLives = Mathf.Clamp(currentLives, 0, _livesSprites.Length);

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
