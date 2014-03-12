using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike
{
    class Skeleton : Enemy
    {
        public Skeleton(Controller controller, Point position, EnemyType enemyType) : base(controller, position, enemyType)
        {
            texture = ContentManager.tSkeleton;
            damage = 2;
            maxHealth = 10;
            health = maxHealth;
        }
    }
}
