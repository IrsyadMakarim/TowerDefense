using Assets.Scripts.Module.Feature.Level;
using Assets.Scripts.Module.Gameplay.Enemy.Model;
using UnityEngine;

namespace Assets.Scripts.Module.Gameplay.Bullet
{
    public class BulletModel : MonoBehaviour
    {
        private int _bulletPower;
        private float _bulletSpeed;
        private float _bulletSplashRadius;

        private EnemyModel _targetEnemy;

        private void FixedUpdate()
        {
            if (_targetEnemy != null)
            {
                if (!_targetEnemy.gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                    _targetEnemy = null;
                    return;
                }

                transform.position = Vector3.MoveTowards(transform.position, _targetEnemy.transform.position, _bulletSpeed * Time.fixedDeltaTime);

                Vector3 direction = _targetEnemy.transform.position - transform.position;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, targetAngle - 90f));
            }

            if (LevelManager.Instance.IsOver)
            {
                return;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_targetEnemy == null)
            {
                return;
            }

            if (collision.gameObject.Equals(_targetEnemy.gameObject))
            {
                gameObject.SetActive(false);
                // Bullet yang memiliki efek splash area
                if (_bulletSplashRadius > 0f)
                {
                    LevelManager.Instance.ExplodeAt(transform.position, _bulletSplashRadius, _bulletPower);
                }
                // Bullet yang hanya single-target
                else
                {
                    _targetEnemy.ReduceEnemyHealth(_bulletPower);
                }

                _targetEnemy = null;
            }
        }

        public void SetProperties(int bulletPower, float bulletSpeed, float bulletSplashRadius)
        {
            _bulletPower = bulletPower;
            _bulletSpeed = bulletSpeed;
            _bulletSplashRadius = bulletSplashRadius;
        }

        public void SetTargetEnemy(EnemyModel enemy)
        {
            _targetEnemy = enemy;
        }
    }
}