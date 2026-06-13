namespace Dungeonlicious.Assets.Script
{
    using System;
    using UnityEngine;

    public class EnemyDeathNotifier : MonoBehaviour
    {
        public event Action OnDeath;

        public void NotifyDeath()
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}
