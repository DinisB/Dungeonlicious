using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dungeonlicious.Assets.Script
{
    public class Slime : IEnemy
    {
        public int Health { get; set; }

        public Slime()
        {
            Health = 20;
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }
    }
}