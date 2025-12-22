using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectT.Gameplay.Weapon.VFX
{
    public class Slash : MonoBehaviour
    {
        private ParticleSystem ps;
        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }
        private void Update()
        {
            if (ps && !ps.IsAlive())
            {
                DestroySelf();
            }
        }
        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
