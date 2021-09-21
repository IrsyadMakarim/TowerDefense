using UnityEngine;
using Assets.Scripts.Module.Gameplay.Tower.Model;
using Assets.Scripts.Module.Gameplay.Tower.View;
using System.Collections.Generic;
using Assets.Scripts.Module.Gameplay.Enemy.Model;
using Assets.Scripts.Module.Gameplay.Bullet;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Module.Feature.Level
{
    public class LevelManager : MonoBehaviour
    {
        //Fungsi Singleton
        private static LevelManager _instance = null;
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LevelManager>();
                }

                return _instance;
            }
        }

        [SerializeField] private Transform _towerUIParent;
        [SerializeField] private GameObject _towerUIPrefab;

        [SerializeField] private TowerModel[] _towerPrefabs;
        [SerializeField] private EnemyModel[] _enemyPrefabs;

        [SerializeField] private Transform[] _enemyPaths;
        [SerializeField] private float _spawnDelay = 5f;

        [SerializeField] private int _maxLives = 3;
        [SerializeField] private int _totalEnemy = 15;

        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _statusInfo;
        [SerializeField] private Text _livesInfo;
        [SerializeField] private Text _totalEnemyInfo;

        private List<TowerModel> _spawnedTowers = new List<TowerModel>();
        private List<EnemyModel> _spawnedEnemies = new List<EnemyModel>();
        private List<BulletModel> _spawnedBullets = new List<BulletModel>();

        private int _currentLives;
        private int _enemyCounter;
        private float _runningSpawnDelay;

        public bool IsOver { get; private set; }

        private void Start()
        {
            InstantiateAllTowerUI();
            SetCurrentLives(_maxLives);
            SetTotalEnemy(_totalEnemy);
        }

        private void Update()
        {
            _runningSpawnDelay -= Time.unscaledDeltaTime;
            if (_runningSpawnDelay <= 0f)
            {
                SpawnEnemy();
                _runningSpawnDelay = _spawnDelay;
            }

            foreach (TowerModel tower in _spawnedTowers)
            {
                tower.CheckNearestEnemy(_spawnedEnemies);
                tower.SeekTarget();
                tower.ShootTarget();
            }

            foreach (EnemyModel enemy in _spawnedEnemies)
            {
                if (!enemy.gameObject.activeSelf)
                {
                    continue;
                }

                if (Vector2.Distance(enemy.transform.position,
                    enemy.TargetPosition) < 0.1f)
                {
                    enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);
                    if (enemy.CurrentPathIndex < _enemyPaths.Length)
                    {
                        enemy.SetTargetPosition
                            (_enemyPaths[enemy.CurrentPathIndex].position);
                    }
                    else
                    {
                        enemy.gameObject.SetActive(false);
                    }
                }
                else
                {
                    enemy.MoveToTarget();
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (IsOver)
            {
                return;
            }
        }

        private void InstantiateAllTowerUI()
        {
            foreach (TowerModel tower in _towerPrefabs)
            {
                GameObject newTowerUIObj = Instantiate
                    (_towerUIPrefab.gameObject, _towerUIParent);

                TowerView newTowerView = newTowerUIObj.
                    GetComponent<TowerView>();

                newTowerView.SetTowerPrefab(tower);
                newTowerView.transform.name = tower.name;
            }
        }

        public void RegisterSpawnedTower(TowerModel tower)
        {
            _spawnedTowers.Add(tower);
        }

        private void SpawnEnemy()
        {
            SetTotalEnemy(--_enemyCounter);
            if (_enemyCounter < 0)
            {
                bool isAllEnemyDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;
                if (isAllEnemyDestroyed)
                {
                    SetGameOver(true);
                }

                return;
            }
            int randomIndex = Random.Range(0, _enemyPrefabs.Length);
            string enemyIndexString = (randomIndex + 1).ToString();

            GameObject newEnemyObj = _spawnedEnemies.Find(
                e => !e.gameObject.activeSelf && e.name.Contains
                (enemyIndexString)
                )?.gameObject;

            if (newEnemyObj == null)
            {
                newEnemyObj = Instantiate(_enemyPrefabs[randomIndex].gameObject);
            }

            EnemyModel newEnemy = newEnemyObj.GetComponent<EnemyModel>();
            if (!_spawnedEnemies.Contains(newEnemy))
            {
                _spawnedEnemies.Add(newEnemy);
            }

            newEnemy.transform.position = _enemyPaths[0].position;
            newEnemy.SetTargetPosition(_enemyPaths[1].position);
            newEnemy.SetCurrentPathIndex(1);
            newEnemy.gameObject.SetActive(true);
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < _enemyPaths.Length - 1; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
            }
        }

        public BulletModel GetBulletFromPool(BulletModel prefab)
        {
            GameObject newBulletObj = _spawnedBullets.Find(
                b => !b.gameObject.activeSelf && b.name.Contains(
                    prefab.name))?.gameObject;

            if (newBulletObj == null)
            {
                newBulletObj = Instantiate(prefab.gameObject);
            }

            BulletModel newBullet = newBulletObj.GetComponent<BulletModel>();
            if (!_spawnedBullets.Contains(newBullet))
            {
                _spawnedBullets.Add(newBullet);
            }

            return newBullet;
        }

        public void ExplodeAt(Vector2 point, float radius, int damage)
        {
            foreach (EnemyModel enemy in _spawnedEnemies)
            {
                if (enemy.gameObject.activeSelf)
                {
                    if (Vector2.Distance(enemy.transform.position, point) <= radius)
                    {
                        enemy.ReduceEnemyHealth(damage);
                    }
                }
            }
        }

        public void ReduceLives(int value)
        {
            SetCurrentLives(_currentLives - value);
            if (_currentLives <= 0)
            {
                SetGameOver(false);
            }
        }

        public void SetCurrentLives(int currentLives)
        {
            _currentLives = Mathf.Max(currentLives, 0);
            _livesInfo.text = $"Lives: {_currentLives}";
        }

        public void SetTotalEnemy(int totalEnemy)
        {
            _enemyCounter = totalEnemy;
            _totalEnemyInfo.text = $"Total Enemy : {Mathf.Max(_enemyCounter, 0)}";
        }  
        
        public void SetGameOver(bool isWin)
        {
            IsOver = true;

            _statusInfo.text = isWin ? "You Win!" : "You Lose!";
            _panel.gameObject.SetActive(true);
        }
    }
}