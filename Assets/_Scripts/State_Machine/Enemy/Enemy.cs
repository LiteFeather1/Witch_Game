using UnityEngine;

public abstract class Enemy : StateMachine.StateMachine, IDeactivatable
{
    [SerializeField] protected EnemyData _data;

    [Header("Base States")]
    [SerializeField] private MovementKnockback _knockbackState;

    [Header("Components")]
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private Health _health;
    [SerializeField] private HitBox _hitBox;

    public EnemyData Data => _data;

    public Health Health => _health;
    public HitBox HitBox => _hitBox;

    public System.Action<Enemy> ReturnToPool { get; set; }
    public System.Action<Enemy> OnDied { get; set; }

    protected void OnEnable()
    {
        _health.OnDamaged += Damaged;
        _health.OnDeath += Died;

        _knockbackState.OnStateComplete += KnockBackComplete;
    }

    protected void OnDisable()
    {
        _health.OnDamaged -= Damaged;
        _health.OnDeath -= Died;

        _knockbackState.OnStateComplete -= KnockBackComplete;
    }

    public virtual void Init(Witch witch) { }

    public virtual void Spawn(float t, float tClamped) 
    {
        _sr.sortingOrder = Random.Range(100, 500);
        _health.ResetHealth(_data.HealthRange.Evaluate(t));
        _hitBox.SetDamage(_data.DamageRange.Evaluate(t));
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        ReturnToPool?.Invoke(this);
    }

    protected abstract void KnockBackComplete();

    private void Damaged(float damage, float knockback, bool crit, Vector2 pos)
    {
        if (pos == _health.Pos)
            return;

        _knockbackState.SetUp((Position - pos).normalized, crit ? knockback * 1.5f : knockback);
        Set(_knockbackState);
    }

    private void Died()
    {
        gameObject.SetActive(false);
        ReturnToPool?.Invoke(this);
        OnDied?.Invoke(this);
    }
}