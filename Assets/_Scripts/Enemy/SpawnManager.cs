﻿using LTFUtils;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Weighter<ObjectPool<Enemy>> _weightedPoolOfEnemies;
    private Dictionary<string, ObjectPool<Enemy>> _enemyToPool;
    [SerializeField] private Vector2Int _maxEnemiesPerBurstRange;
    [SerializeField] private Vector2Int _minEnemiesPerBurstRange;
    [SerializeField] private Vector2Int _enemiesPerBurstOffset;
    [SerializeField] private Vector2 _maxSpawnTimeRange;
    [SerializeField] private Vector2 _minSpawnTimeRange;
    [SerializeField] private Vector2 _spawnTimeOffset;
    private float _spawnTime;
    private float _elapsedTime;
    [SerializeField] private BoxCollider2D _spawnArea;

    [Header("Candy")]
    [SerializeField] private ObjectPool<CurrencyBehaviour> _currencyPool;
    [SerializeField] private float _candySpawnOffset = 0.04f;
    [SerializeField] private CompositeValue _chanceToExtraCandy = new(0f);

    public List<Enemy> ActiveEnemies { get; private set; } = new();

    public CompositeValue ChanceToExtraCandy => _chanceToExtraCandy;

    private void Start()
    {
        _currencyPool.InitPool();
        foreach (var currency in _currencyPool.Objects)
        {
            CurrencyCreated(currency);
        }
        _currencyPool.ObjectCreated += CurrencyCreated;

        _enemyToPool = new(_weightedPoolOfEnemies.Count);
        for (int i = 0; i < _weightedPoolOfEnemies.Count; i++)
        {
            var pool = _weightedPoolOfEnemies.Objects[i].Object;
            pool.InitPool();
            foreach (var enemy in pool.Objects)
            {
                EnemyCreated(enemy);
            }
            _enemyToPool.Add(pool.Object.Name, pool);
            pool.ObjectCreated += EnemyCreated;
        }

        _spawnTime = Random.Range(2.75f, 4f);
    }

    private void OnDestroy()
    {
        foreach (var currency in _currencyPool.Objects)
        {
            currency.ReturnToPool -= ReturnCurrencyToPool;
        }
        _currencyPool.ObjectCreated -= CurrencyCreated;

        for (int i = 0; i < _weightedPoolOfEnemies.Count; i++)
        {
            var pool = _weightedPoolOfEnemies.Objects[i].Object;
            foreach (var enemy in pool.Objects)
            {
                enemy.ReturnToPool -= ReturnEnemyToPool;
                enemy.OnDied -= EnemyDied;
            }
            pool.ObjectCreated -= EnemyCreated;
        }
    }

    public void Tick(float t, float tClamped)
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _spawnTime)
        {
            _elapsedTime = 0f;
            var spawnTime = Random.Range(_minSpawnTimeRange.Evaluate(tClamped), _maxSpawnTimeRange.Evaluate(tClamped));
            var offSetTime = _spawnTimeOffset.Evaluate(tClamped);
            _spawnTime = spawnTime + Random.Range(-offSetTime, offSetTime);

            var minBurstAmount = _minEnemiesPerBurstRange.Evaluate(t);
            var maxBurstAmount = _maxEnemiesPerBurstRange.Evaluate(t);
            var offSetBurstT = _enemiesPerBurstOffset.Evaluate(t);
            var spawnAmount = Random.Range(minBurstAmount, maxBurstAmount) + Random.Range(0, offSetBurstT);
            for (int i = 0; i < spawnAmount; i++)
            {
                var enemy = _weightedPoolOfEnemies.GetWeightedObject().Object.GetObject();
                var randX = Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x);
                var randY = Random.Range(_spawnArea.bounds.min.y, _spawnArea.bounds.max.y);
                enemy.transform.localPosition = new(randX, randY);
                enemy.Spawn(tClamped);
                ActiveEnemies.Add(enemy);
            }
        }
    }

    private void CurrencyCreated(CurrencyBehaviour currency)
    {
        currency.Init(GameManager.Instance.Witch);
        currency.ReturnToPool += ReturnCurrencyToPool;
    }

    private void ReturnCurrencyToPool(CurrencyBehaviour currency)
    {
        _currencyPool.ReturnObject(currency);   
    }

    private void EnemyDied(Vector2 pos)
    {
        SpawnCandy(pos);
        if (Random.value < _chanceToExtraCandy.Value)
            SpawnCandy(pos);
    }

    private void SpawnCandy(Vector2 pos)
    {
        var currency = _currencyPool.GetObject();
        var randX = Random.Range(-_candySpawnOffset, _candySpawnOffset);
        var randY = Random.Range(-_candySpawnOffset, _candySpawnOffset);
        currency.transform.position = pos + new Vector2(randX, randY);
        currency.gameObject.SetActive(true);
    }

    private void EnemyCreated(Enemy enemy)
    {
        enemy.Init(GameManager.Instance.Witch);
        enemy.ReturnToPool += ReturnEnemyToPool;
        enemy.OnDied += EnemyDied;
    }

    private void ReturnEnemyToPool(Enemy enemy)
    {
        ActiveEnemies.Remove(enemy);
        _enemyToPool[enemy.Name].ReturnObject(enemy);
    }
}
