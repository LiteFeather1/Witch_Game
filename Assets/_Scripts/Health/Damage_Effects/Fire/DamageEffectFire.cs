﻿public class DamageEffectFire : DamageEffect
{
    private readonly float _damage;
    private readonly float _tickTime;
    private int _ticks;

    public override int ID => (int)IDamageEffect.DamageEffectID.FIRE_ID;

    public DamageEffectFire(float duration, float damage, int tickAmount) : base(duration)
    {
        _damage = damage;
        _tickTime = duration / tickAmount;
        _ticks = 0;
    }

    public override bool Tick(IDamageable damageable, float delta)
    {
        if (_elapsedTime > _tickTime * _ticks)
        {
            _ticks++;
            UnityEngine.Debug.Log("Tick");
            damageable.TakeDamage(_damage, 0f, false, damageable.Pos);
        }

        return base.Tick(damageable, delta);
    }
}