using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI _scoreText, _gameOverText, _restartGameText, _ammoText, _waveSpawnText, _waveNumberText;
    [SerializeField] private Image _livesImage;
    [SerializeField] private Sprite[] _livesSprites;
    [SerializeField] private Slider _boostSlider;
    private readonly WaitForSeconds _flickerDelay = new(0.25f);
    private readonly WaitForSeconds _boostDelay = new(0.05f);
    private readonly WaitForSeconds _waveDelay = new(1f);
    private readonly WaitForSeconds _nowDelay = new(0.5f);
    private float _boostSliderMaxValue = 100f;
    private bool _canUseBoost = true;
    private bool _losingFuel = false;
    private bool _gainingFuel = false;

    private void Start()
    {
        _boostSlider.value = _boostSliderMaxValue;
        UpdateScoreText(0);
        UpdateAmmoAmount(15);
        UpdateWaveText(1);
    }

    public void UpdateScoreText(int score)
    {
        _scoreText.text = "Score: " + score.ToString();
    }

    public void UpdateWaveText(int wave)
    {
        _waveNumberText.text = "Wave: " + wave.ToString();
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

    public bool CanUseThrusterBoost()
    {
        return _canUseBoost;
    }

    public IEnumerator ThrusterBoostSliderDownRoutine()
    {
        if (_losingFuel)
            yield break;

        _losingFuel = true;
        _gainingFuel = false;

        while (_losingFuel)
        {
            _boostSlider.value -= 1.25f;
            yield return _boostDelay;

            if (_boostSlider.value <= 0f)
            {
                _boostSlider.value = 0f;
                _canUseBoost = false;
                _losingFuel = false;
            }
        }
    }

    public IEnumerator ThrusterBoostSliderUpRoutine()
    {
        if (_gainingFuel || _boostSlider.value == 100f)
            yield break;

        _gainingFuel = true;
        _losingFuel = false;

        while (_gainingFuel && !_canUseBoost)
        {
            _boostSlider.value += 1f;
            yield return _boostDelay;

            if (_boostSlider.value == _boostSliderMaxValue)
            {
                _canUseBoost = true;
                _gainingFuel = false;
            }
        }
    }
    public IEnumerator NextWaveSpawnRoutine(int wave)
    {
        _waveSpawnText.gameObject.SetActive(true);

        for (int i = 3; i >= 0; i--)
        {
            string message = (i > 0) ? $"WAVE {wave} STARTS IN {i}!" : $"WAVE {wave} STARTS NOW!";
            _waveSpawnText.text = message;

            yield return (i > 0) ? _waveDelay : _nowDelay;
        }

        _waveSpawnText.gameObject.SetActive(false);
    }
}
