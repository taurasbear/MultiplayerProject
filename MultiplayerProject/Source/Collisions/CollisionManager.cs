using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using MultiplayerProject.Source.Networking.Server;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.GameObjects.Enemy;
using MultiplayerProject.Source.GameObjects.Iterator;
using MultiplayerProject.Source.GameObjects.Iterator.Laser;
using MultiplayerProject.Source.Helpers;
using MultiplayerProject.Source.Helpers.Factories;
using System;
using System.Collections.Generic;


namespace MultiplayerProject.Source
{
    public class CollisionManager
    {
        public CollisionManager()
        {
        }
        public enum CollisionType
        {
            LaserToPlayer,
            LaserToEnemy,
            EnemyToPlayer // New collision type for enemy-player contact
        }

        public struct Collision
        {
            public CollisionType CollisionType { get; set; }
            public string LaserID { get; set; }
            public string AttackingPlayerID { get; set; }
            public string DefeatedPlayerID { get; set; } // Null if enemy was shot
            public string DefeatedEnemyID { get; set; } // Null if player was shot
            public string EnemyID { get; set; } // For EnemyToPlayer collisions

            public Collision(CollisionType collisionType, string laserID, string attackingPlayerID, string defeatedPlayerID, string defeatedEnemyID)
            {
                CollisionType = collisionType;
                LaserID = laserID;
                AttackingPlayerID = attackingPlayerID;
                DefeatedPlayerID = defeatedPlayerID;
                DefeatedEnemyID = defeatedEnemyID;
                EnemyID = null;
            }

            public Collision(CollisionType collisionType, string playerID, string enemyID)
            {
                CollisionType = collisionType;
                LaserID = null;
                AttackingPlayerID = null;
                DefeatedPlayerID = playerID;
                DefeatedEnemyID = null;
                EnemyID = enemyID;
            }
        }

        /// <summary>
        /// Check collisions using GameObjectCollection and Iterator pattern
        /// </summary>
        public List<Collision> CheckCollision(GameObjectCollection gameObjectCollection)
        {
            List<Collision> collisions = new List<Collision>();

            CheckLaserCollisions(gameObjectCollection, collisions);
            CheckEnemyToPlayerCollisions(gameObjectCollection, collisions);

            return collisions;
        }

        /// <summary>
        /// Check laser-to-player and laser-to-enemy collisions using iterators
        /// </summary>
        private void CheckLaserCollisions(GameObjectCollection gameObjectCollection, List<Collision> collisions)
        {
            var laserIterator = gameObjectCollection.CreateLaserIterator();
            
            while (laserIterator.HasMore())
            {
                Laser laser = laserIterator.GetNext();
                if (laser == null || !laser.Active)
                    continue;

                Rectangle laserRectangle = new Rectangle(
                    (int)laser.Position.X,
                    (int)laser.Position.Y,
                    laser.Width,
                    laser.Height);

                bool laserStillActive = true;

                var playerIterator = gameObjectCollection.CreatePlayerIterator();
                while (playerIterator.HasMore() && laserStillActive)
                {
                    Player player = playerIterator.GetNext();
                    if (player == null || !player.Active)
                        continue;

                    if (player.IsInvincible())
                        continue;

                    Rectangle playerRectangle = new Rectangle(
                        (int)player.Position.X,
                        (int)player.Position.Y,
                        player.Width,
                        player.Height);

                    if (laser.PlayerFiredID != player.NetworkID && laserRectangle.Intersects(playerRectangle))
                    {
                        collisions.Add(new Collision(CollisionType.LaserToPlayer, laser.LaserID, laser.PlayerFiredID, player.NetworkID, ""));
                        
                        int laserDamage = (int)laser.Damage;
                        player.HandleLaserCollision(laserDamage);
                        
                        laserStillActive = false;
                        break;
                    }
                }

                if (laserStillActive)
                {
                    var enemyIterator = gameObjectCollection.CreateEnemyIterator();
                    while (enemyIterator.HasMore() && laserStillActive)
                    {
                        Enemy enemy = enemyIterator.GetNext();
                        if (enemy == null || !enemy.Active)
                            continue;

                        Rectangle enemyRectangle = new Rectangle(
                            (int)enemy.Position.X,
                            (int)enemy.Position.Y,
                            enemy.Width,
                            enemy.Height);

                        if (laserRectangle.Intersects(enemyRectangle))
                        {
                            collisions.Add(new Collision(CollisionType.LaserToEnemy, laser.LaserID, laser.PlayerFiredID, "", enemy.EnemyID));
                            laserStillActive = false;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check for enemy-to-player collisions using iterators. Players take damage when touching enemies.
        /// </summary>
        private void CheckEnemyToPlayerCollisions(GameObjectCollection gameObjectCollection, List<Collision> collisions)
        {
            const int ENEMY_CONTACT_DAMAGE = 20; // Damage dealt by touching an enemy

            var playerIterator = gameObjectCollection.CreatePlayerIterator();
            
            while (playerIterator.HasMore())
            {
                Player player = playerIterator.GetNext();
                if (player == null || !player.Active)
                    continue;

                if (player.IsInvincible())
                    continue;

                Rectangle playerRectangle = new Rectangle(
                    (int)player.Position.X,
                    (int)player.Position.Y,
                    player.Width,
                    player.Height);

                var enemyIterator = gameObjectCollection.CreateEnemyIterator();
                bool playerHitEnemy = false;
                
                while (enemyIterator.HasMore() && !playerHitEnemy)
                {
                    Enemy enemy = enemyIterator.GetNext();
                    if (enemy == null || !enemy.Active)
                        continue;

                    Rectangle enemyRectangle = new Rectangle(
                        (int)enemy.Position.X,
                        (int)enemy.Position.Y,
                        enemy.Width,
                        enemy.Height);

                    if (playerRectangle.Intersects(enemyRectangle))
                    {
                        collisions.Add(new Collision(CollisionType.EnemyToPlayer, player.NetworkID, enemy.EnemyID));
                        player.HandleEnemyCollision(ENEMY_CONTACT_DAMAGE);
                        playerHitEnemy = true;
                        break;
                    }
                }
            }
        }

        public void Draw(GraphicsDevice device, SpriteBatch spriteBatch, List<Enemy> enemies, List<Laser> lasers)
        {
            foreach (Enemy enemy in enemies)
            {
                Texture2D texture = new Texture2D(device, enemy.Width, enemy.Height);
                texture.CreateBorder(1, Color.Red);
                spriteBatch.Draw(texture, enemy.CentrePosition, Color.White);
            }

            foreach (Laser laser in lasers)
            {
                Texture2D texture = new Texture2D(device, laser.Width, laser.Height);
                texture.CreateBorder(1, Color.Blue);
                spriteBatch.Draw(texture, laser.Position, Color.White);
            }
        }
    }

    static class Utilities
    {
        public static void CreateBorder(this Texture2D texture, int borderWidth, Color borderColor)
        {
            Color[] colors = new Color[texture.Width * texture.Height];

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    bool colored = false;
                    for (int i = 0; i <= borderWidth; i++)
                    {
                        if (x == i || y == i || x == texture.Width - 1 - i || y == texture.Height - 1 - i)
                        {
                            colors[x + y * texture.Width] = borderColor;
                            colored = true;
                            break;
                        }
                    }

                    if (colored == false)
                        colors[x + y * texture.Width] = Color.Transparent;
                }
            }

            texture.SetData(colors);
        }
    }
}
