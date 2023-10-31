﻿using LTFUtils;
using RetroAnimation;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private ObjectPool<Bullet> _bulletPool;
    [SerializeField] private ObjectPool<DisableCallBack> _particlePool;
    [SerializeField] private ObjectPool<FlipBook> _bulletDamage;

    public System.Action<float> OnDamageAppplied;

    private void Awake()
    {
        _bulletPool.InitPool();
        foreach (Bullet bullet in _bulletPool.Objects)
        {
            BulletCreated(bullet);
        }
        _bulletPool.ObjectCreated += BulletCreated;

        _particlePool.InitPool();
        foreach (var particle in _particlePool.Objects)
        {
            ParticleCreated(particle);
        }

        _particlePool.ObjectCreated += ParticleCreated;

        _bulletDamage.InitPool();
        foreach (var explosion in _bulletDamage.Objects)
        {
            DamageExplosionCreated(explosion);
        }
        _bulletDamage.ObjectCreated += DamageExplosionCreated;
    }

    private void OnDestroy()
    {
        foreach (var bullet in _bulletPool.Objects)
        {
            bullet.ReturnToPool -= ReturnBulletToPool;
            bullet.Hitbox.OnDamageAppplied -= DamageAppplied;
        }
        _bulletPool.ObjectCreated -= BulletCreated;

        foreach (var particle in _particlePool.Objects)
        {
            particle.Disabled -= ReturnParticleToPool;
        }
        _particlePool.ObjectCreated -= ParticleCreated; ;

        foreach (var explosion in _bulletDamage.Objects)
        {
            explosion.OnAnimationFinished -= ReturnExplosionToPool;
        }
        _bulletDamage.ObjectCreated -= DamageExplosionCreated;
    }

    public void ShootRoutine(float damage,
                                float critChance,
                                float critMultiplier,
                                float knockback,
                                float size,
                                float speed,
                                int pierce,
                                int bounce,
                                float duration,
                                float angle,
                                float timeToCompleteShooting,
                                int bulletAmount,
                                float burstAmount,
                                float separationPerBullet)
    {
        StartCoroutine(Shot_CO(damage: damage,
                               critChance: critChance,
                               critMultiplier: critMultiplier,
                               knockback: knockback,
                               size: size,
                               speed: speed,
                               pierce: pierce,
                               bounce: bounce,
                               duration: duration,
                               angle: angle,
                               timeToCompleteShooting: timeToCompleteShooting,
                               bulletAmount: bulletAmount,
                               burstAmount: burstAmount,
                               separationPerBullet: separationPerBullet));
    }

    public void ShootBullet(float damage, float critChance, float critMultiplier, float knockback , float size, 
        float speed, int pierce, int bounce, float duration, float angle)
    {
        var bullet = _bulletPool.GetObject();
        var particle = _particlePool.GetObject();
        particle.gameObject.SetActive(true);
        bullet.transform.SetPositionAndRotation(_firePoint.position,
                                                     Quaternion.Euler(0f, 0f, angle));
        bullet.transform.localScale = Vector3.one * size;
        bullet.AttachDisable(particle);
        bullet.gameObject.SetActive(true);
        bullet.Hitbox.SetDamage(damage);
        bullet.Hitbox.SetCritChance(critChance);
        bullet.Hitbox.SetCritMultiplier(critMultiplier);
        bullet.Hitbox.SetKnockback(knockback);
        bullet.Projectile.Shoot(speed, (Vector2)bullet.transform.right,
                                pierce, bounce, duration);
    }

    private IEnumerator Shot_CO(float damage,
                                float critChance,
                                float critMultiplier,
                                float knockback,
                                float size,
                                float speed,
                                int pierce,
                                int bounce,
                                float duration,
                                float angle,
                                float timeToCompleteShooting,
                                int bulletAmount,
                                float burstAmount,
                                float separationPerBullet)
    {
        WaitForSeconds yieldBetweenBurst = new(timeToCompleteShooting / burstAmount);
        for (int i = 0; i < burstAmount; i++)
        {
            for (int j = 0; j < bulletAmount; j++)
            {
                ShootBullet(damage: damage,
                            critChance: critChance,
                            critMultiplier: critMultiplier,
                            knockback: knockback,
                            size: size,
                            speed: speed,
                            pierce: pierce,
                            bounce: bounce,
                            duration: duration,
                            angle: GetAngle(j, bulletAmount, separationPerBullet) + angle);
            }
            yield return yieldBetweenBurst;
        }
    }

    private float GetAngle(int bulletIndex, int bulletAmount, float separationPerBullet)
    {
        float totalAngle = (bulletAmount - 1) * separationPerBullet * .5f;
        float center = LTFHelpers_Math.AngleBetweenTwoPoints(transform.position, transform.position - _firePoint.right);
        float minAngle = center - totalAngle;
        return minAngle + (bulletIndex * separationPerBullet);
    }

    private void BulletCreated(Bullet bullet)
    {
        bullet.ReturnToPool += ReturnBulletToPool;
        bullet.Hitbox.OnDamageAppplied += DamageAppplied;
    }

    private void ReturnBulletToPool(Bullet bullet)
    {
        _bulletPool.ReturnObject(bullet);
    }

    private void DamageAppplied(float damage, Vector2 pos)
    {
        OnDamageAppplied?.Invoke(damage);

        var bulletExplosion = _bulletDamage.GetObject();
        bulletExplosion.transform.position = pos;
        bulletExplosion.Play();
        bulletExplosion.gameObject.SetActive(true);

    }

    private void ParticleCreated(DisableCallBack particle)
    {
        particle.Disabled += ReturnParticleToPool;
    }

    private void ReturnParticleToPool(DisableCallBack particle)
    {
        _particlePool.ReturnObject(particle);
    }

    private void DamageExplosionCreated(FlipBook explosion)
    {
        explosion.OnAnimationFinished += ReturnExplosionToPool;
    }

    private void ReturnExplosionToPool(FlipBook explosion)
    {
        explosion.gameObject.SetActive(false);
        _bulletDamage.ReturnObject(explosion);
    }
}
