using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Witch _witch;

    [Header("Settings")]
    [SerializeField] private AudioClip _music;
    [SerializeField] private float _slowDownScale = .25f;
    [SerializeField] private float _playTime;
    [SerializeField] private float _timeForMaxDifficult = 360f;

    [Header("Managers")]
    [SerializeField] private Camera _camera;
    [SerializeField] private CameraShake _shake;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private EndScreenManager _endScreenManager;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private CardManager _cardManager;

    [Header("Recycle Effects")]
    [SerializeField] private CompositeValue _damageEnemiesOnRecycle;
    [SerializeField] private CompositeValue _healOnRecycle;
    [SerializeField] private CompositeValue _addCurrencyOnRecycle;
    [SerializeField] private CompositeValue _refundOnRecycle = new(0f);

    [Header("On Card Played Effect")]
    [SerializeField] private CompositeValue _damageEnemiesOnCardPlayed;
    [SerializeField] private CompositeValue _healOnCardPlayed;
    [SerializeField] private CompositeValue _refundOnCardPlayed;

    private IEnumerator _slowPitch;

    public static GameManager Instance { get; private set; }
    public static InputMapping Inputs { get; private set; }

    public Witch Witch => _witch;
    public Camera Camera => _camera;
    public SpawnManager SpawnManager => _spawnManager;
    public CardManager CardManager => _cardManager;

    public CompositeValue DamageEnemiesOnRecycle => _damageEnemiesOnRecycle;
    public CompositeValue HealOnRecycle => _healOnRecycle;
    public CompositeValue AddCurrencyOnRecycle => _addCurrencyOnRecycle;
    public CompositeValue RefundOnRecycle => _refundOnRecycle;

    public CompositeValue DamageEnemiesOnCardPlayed => _damageEnemiesOnCardPlayed;
    public CompositeValue HealOnCardPlayed => _healOnCardPlayed;
    public CompositeValue RefundOnCardPlayed => _refundOnCardPlayed;

    private void Awake()
    {
        Instance = this;
        Inputs = new();
        Inputs.Enable();

        Inputs.Player.Pause_UnPause.performed += PauseUnpause;
        Inputs.Player.Mute_UnMute.performed += MuteUnMute;

        _witch.OnDamaged += _shake.ShakeStrong;
        _witch.Health.OnDeath += WitchDied;

        _cardManager.OnCardHovered += SlowDown;
        _cardManager.OnCardUnHovered += UnSlowDown;
        _cardManager.Recycler.OnCardUsed += CardRecycled;
        _cardManager.PlayArea.OnPowerPlayed += CardPlayed;

        _spawnManager.EnemyHurt += _shake.Shake;

        _uiManager.BindToWitch(_witch);

        _slowPitch = SetPitch(1f);
    }

    private void Start()
    {
        AudioManager.Instance.MusicSource.clip = _music;
        AudioManager.Instance.MusicSource.Play();
    }

    private void Update()
    {
        _playTime += Time.deltaTime;

        var t = _playTime / _timeForMaxDifficult;
        var tClamped = t;
        if (tClamped > 1f)
            tClamped = 1f;

        _spawnManager.Tick(t, tClamped);
        _uiManager.UpdateTime(_playTime);
    }

    private void OnDestroy()
    {
        Inputs.Player.Pause_UnPause.performed -= PauseUnpause;
        Inputs.Player.Mute_UnMute.performed -= MuteUnMute;

        _witch.OnDamaged -= _shake.ShakeStrong;
        _witch.Health.OnDeath -= WitchDied;

        _cardManager.OnCardHovered -= SlowDown;
        _cardManager.OnCardUnHovered -= UnSlowDown;
        _cardManager.Recycler.OnCardUsed -= CardRecycled;
        _cardManager.PlayArea.OnPowerPlayed -= CardPlayed;

        _spawnManager.EnemyHurt -= _shake.Shake;
     
        _uiManager.UnBindToWitch(_witch);
    }

    public void PauseUnpause()
    {
        bool paused = Time.timeScale > 0f;
        Time.timeScale = paused ? 0f : 1f;
        _uiManager.SetPauseScreen(paused);
    }

    public void LoadSplashScreen()
    {
        SceneManager.LoadScene(0);
        AudioManager.Instance.MusicSource.Stop();
        Time.timeScale = 1f;
    }

    private void WitchDied()
    {
        Inputs.Player.Pause_UnPause.performed -= PauseUnpause;
        _endScreenManager.gameObject.SetActive(true);
        _uiManager.GameUi.SetActive(false);
        _endScreenManager.SetTexts(_uiManager.TimeText,
                                   _spawnManager.EnemiesDied,
                                   _cardManager.Recycler.CardsRecycled,
                                   _witch.TotalCurrencyGained);
    }

    private void PauseUnpause(InputAction.CallbackContext ctx) => PauseUnpause();

    private void MuteUnMute(InputAction.CallbackContext ctx)
    {
        AudioListener.volume = AudioListener.volume > 0.1f ? 0f : 1f;
    }

    private void SlowDown()
    {
        Time.timeScale = _slowDownScale;
        StopCoroutine(_slowPitch);
        _slowPitch = SetPitch(0.75f);
        StartCoroutine(_slowPitch);
    }

    private void UnSlowDown()
    {
        Time.timeScale = 1f;
        StopCoroutine(_slowPitch);
        _slowPitch = SetPitch(1f);
        StartCoroutine(_slowPitch);
    }

    private IEnumerator SetPitch(float pitch)
    {
        float eTime = 0f;
        float currentPitch = AudioManager.Instance.MusicSource.pitch;
        while (eTime < 0.25f)
        {
            float t = eTime / 0.5f;
            AudioManager.Instance.MusicSource.pitch = Mathf.Lerp(currentPitch, pitch, t);
            AudioManager.Instance.SFXSource.pitch = Mathf.Lerp(currentPitch, pitch, t);
            eTime += Time.unscaledDeltaTime;
            yield return null;
        }
        AudioManager.Instance.MusicSource.pitch = pitch;
        AudioManager.Instance.SFXSource.pitch = pitch;
    }

    private void DamageEveryEnemy(float damage)
    {
        if (damage < 0.01f)
            return;

        // Coping array so we don't get out of index  // 
        var enemies = _spawnManager.ActiveEnemies.ToArray();
        for (int i = 0; i < enemies.Length; i++)
            enemies[i].Health.TakeDamage(damage, 0f, false, enemies[i].transform.position);
    }

    private void CardRecycled()
    {
        DamageEveryEnemy(_damageEnemiesOnRecycle);

        _witch.Health.Heal(_healOnRecycle);
        _witch.ModifyCurrency((int)_addCurrencyOnRecycle);

        _cardManager.CardRefundDrawer(_refundOnRecycle);
    }

    private void CardPlayed(PowerUp powerUp)
    {
        _endScreenManager.AddCard(powerUp);

        DamageEveryEnemy(_damageEnemiesOnCardPlayed);

        _witch.Health.Heal(_healOnCardPlayed);

        _cardManager.CardRefundDrawer(_refundOnCardPlayed);
    }
}
