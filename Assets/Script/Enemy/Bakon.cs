using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dungeonlicious.Assets.Script
{
    public class Bakon : IEnemy
    {
        public int Health { get; set; }

        public Bakon()
        {
            Health = 60;
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }
    }
}