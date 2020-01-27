using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapLoader
{
    public class PlayerSpawn : MonoBehaviour
    {
        public WeaponType startWeapon;

        public void Start()
        {
        }
    }

    public class MilkSpawn : MonoBehaviour
    {
        public void Start()
        {
        }
    }

    public class EnemySpawn : MonoBehaviour
    {
        public WeaponType startWeapon;

        public void Start()
        {
        }
    }

    public class BarrelSpawn : MonoBehaviour
    {
        public void Start()
        {
        }
    }

    public enum WeaponType { None, Shotgun, Boomer, Pistol, GrapplingGun, MicroUZI };

    public class WeaponSpawn : MonoBehaviour
    {
        public WeaponType type;

        public void Start()
        {
        }
    }



    public class ControlGameObject : MonoBehaviour
    {
        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void SetActive()
        {
            gameObject.SetActive(true);
        }

        public void SetInactive()
        {
            gameObject.SetActive(false);
        }
    }
}
