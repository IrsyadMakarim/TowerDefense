using System.Collections;
using UnityEngine;
using Assets.Scripts.Module.Gameplay.Tower.Model;

namespace Assets.Scripts.Module.Gameplay.Tower.Controller
{
    public class TowerController : MonoBehaviour
    {
        private TowerModel _placedTower;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_placedTower != null)
            {
                return;
            }
            TowerModel tower = collision.GetComponent<TowerModel>();
            if (tower !=  null)
            {
                tower.SetPlacePosition(transform.position);
                _placedTower = tower;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (_placedTower == null)
            {
                return;
            }

            _placedTower.SetPlacePosition(null);
            _placedTower = null;
        }
    }
}