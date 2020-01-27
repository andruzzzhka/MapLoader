using System;
using System.Collections;
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
            StartCoroutine(WaitForLevel());
        }

        private IEnumerator WaitForLevel()
        {
            yield return null;

            yield return new WaitWhile(() => LevelAssetsLoader.Instance.isLoadingLevel);

            Prefabs.player.transform.position = transform.position;

            Prefabs.player.SetActive(true);
            Prefabs.camera.SetActive(true);

            if (startWeapon != WeaponType.None)
            {
                GameObject currentGun = Prefabs.GetWeapon(startWeapon);

                Prefabs.player.GetComponentInChildren<DetectWeapons>().ForcePickup(currentGun);
            }

            Destroy(gameObject);
        }
    }

    public class MilkSpawn : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(WaitForLevel());
        }

        private IEnumerator WaitForLevel()
        {
            yield return null;

            yield return new WaitWhile(() => LevelAssetsLoader.Instance.isLoadingLevel);

            var clone = Instantiate(Prefabs.milk);

            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;

            clone.SetActive(true);

            Destroy(gameObject);
        }
    }

    public class EnemySpawn : MonoBehaviour
    {
        public WeaponType startWeapon;

        public void Start()
        {
            StartCoroutine(WaitForLevel());
        }

        private IEnumerator WaitForLevel()
        {
            yield return null;

            yield return new WaitWhile(() => LevelAssetsLoader.Instance.isLoadingLevel);

            var clone = Instantiate(Prefabs.enemy).GetComponent<Enemy>();

            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;

            clone.gameObject.SetActive(true);

            GameObject currentGun = clone.currentGun;

            Destroy(currentGun);

            if(startWeapon != WeaponType.None)
            {
                GameObject gun = Prefabs.GetWeapon(startWeapon);

                clone.startGun = gun;
                clone.GiveGun();
            }

            Destroy(gameObject);
        }
    }

    public class BarrelSpawn : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(WaitForLevel());
        }

        private IEnumerator WaitForLevel()
        {
            yield return null;

            yield return new WaitWhile(() => LevelAssetsLoader.Instance.isLoadingLevel);

            var clone = Instantiate(Prefabs.barrel);

            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;

            clone.gameObject.SetActive(true);
            Destroy(gameObject);
        }
    }

    public enum WeaponType { None, Shotgun, Boomer, Pistol, GrapplingGun, MicroUZI };

    public class WeaponSpawn : MonoBehaviour
    {
        public WeaponType type;

        public void Start()
        {
            StartCoroutine(WaitForLevel());
        }

        private IEnumerator WaitForLevel()
        {
            yield return null;

            yield return new WaitWhile(() => LevelAssetsLoader.Instance.isLoadingLevel);

            var clone = Prefabs.GetWeapon(type);

            if (clone != null)
            {
                clone.transform.position = transform.position;
                clone.transform.rotation = transform.rotation;

                clone.SetActive(true);
            }

            Destroy(gameObject);
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
