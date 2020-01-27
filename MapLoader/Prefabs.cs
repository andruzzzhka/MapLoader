using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapLoader
{
    public class Prefabs : MonoBehaviour
    {
        public static GameObject player;
        public static GameObject camera;
        public static GameObject enemy;
        public static GameObject milk;
        public static GameObject barrel;

        public static GameObject shotgun;
        public static GameObject boomer;
        public static GameObject pistol;
        public static GameObject grapplingGun;
        public static GameObject microUZI;

        public static GameObject GetWeapon(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Shotgun:
                    {
                        var result = Instantiate(shotgun);
                        result.SetActive(true); 
                        return result;
                    }
                case WeaponType.Boomer:
                    {
                        var result = Instantiate(boomer);
                        result.SetActive(true);
                        return result;
                    }
                case WeaponType.Pistol:
                    {
                        var result = Instantiate(pistol);
                        result.SetActive(true);
                        return result;
                    }
                case WeaponType.GrapplingGun:
                    {
                        var result = Instantiate(grapplingGun);
                        result.SetActive(true);
                        return result;
                    }
                case WeaponType.MicroUZI:
                    {
                        var result = Instantiate(microUZI);
                        result.SetActive(true);
                        return result;
                    }
                case WeaponType.None:
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
