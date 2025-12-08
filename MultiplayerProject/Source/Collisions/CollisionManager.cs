using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerProject.Source.Helpers;
using System;
using System.Collections.Generic;
//using MultiplayerProject.Source.Networking.Server;
using MultiplayerProject.Source.GameObjects;
using MultiplayerProject.Source.Helpers.Factories;
using MultiplayerProject.Source.GameObjects.Enemy;


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

        // NOTE: The explosion-handling overload was removed - use the main CheckCollision(players, enemies, lasers)
        // if you need explosion creation, call ExplosionManager from the caller with player lookup available.

        public List<Collision> CheckCollision(List<Player> players, List<Enemy> enemies, List<Laser> lasers)
        {
            List<Collision> collisions = new List<Collision>();
            bool laserStillActive;

            for (int iLaser = 0; iLaser < lasers.Count; iLaser++) // Loop through every active laser
            {
                Rectangle laserRectangle = new Rectangle(
                    (int)lasers[iLaser].Position.X,
                    (int)lasers[iLaser].Position.Y,
                    lasers[iLaser].Width,
                    lasers[iLaser].Height);
                laserStillActive = true;

                for (int iPlayer = 0; iPlayer < players.Count; iPlayer++) // Loop through every active player
                {
                    // Skip if player is invincible
                    if (players[iPlayer].IsInvincible())
                        continue;

                    Rectangle playerRectangle = new Rectangle(
                    (int)players[iPlayer].Position.X,
                    (int)players[iPlayer].Position.Y,
                    players[iPlayer].Width,
                    players[iPlayer].Height);

                    if (lasers[iLaser].PlayerFiredID != players[iPlayer].NetworkID // Make sure we don't check for collisions which the player that fired it
                        && laserRectangle.Intersects(playerRectangle)) 
                    {
                        collisions.Add(new Collision(CollisionType.LaserToPlayer, lasers[iLaser].LaserID, lasers[iLaser].PlayerFiredID, players[iPlayer].NetworkID, ""));
                        
                        // Apply laser damage through state pattern
                        int laserDamage = (int)lasers[iLaser].Damage;
                        players[iPlayer].HandleLaserCollision(laserDamage);
                        Console.WriteLine($"SUCCESSFUL LASER/PLAYER INTERSECTION - Player took {laserDamage} damage");
                        
                        laserStillActive = false;
                        break; // If collided don't check for more player collisions
                    }
                }

                if (laserStillActive)
                {
                    // Loop through every parent enemy
                    for (int iParentEnemy = 0; iParentEnemy < enemies.Count; iParentEnemy++)
                    {
                        Enemy currentEnemy = enemies[iParentEnemy];

                        // Check collision with parent enemy
                        Rectangle parentEnemyRectangle = new Rectangle(
                            (int)currentEnemy.Position.X,
                            (int)currentEnemy.Position.Y,
                            currentEnemy.Width,
                            currentEnemy.Height);

                        if (laserRectangle.Intersects(parentEnemyRectangle))
                        {
                            collisions.Add(new Collision(CollisionType.LaserToEnemy, lasers[iLaser].LaserID, lasers[iLaser].PlayerFiredID, "", currentEnemy.EnemyID));
                            laserStillActive = false; // Laser is used up
                            break; // Stop checking this laser against other enemies
                        }

                        // Check collision with minions of the current parent enemy
                        foreach (var minion in currentEnemy.Minions)
                        {
                            Rectangle minionRectangle = new Rectangle((int)minion.Position.X, (int)minion.Position.Y, minion.Width, minion.Height);
                            if (laserRectangle.Intersects(minionRectangle))
                            {
                                collisions.Add(new Collision(CollisionType.LaserToEnemy, lasers[iLaser].LaserID, lasers[iLaser].PlayerFiredID, "", minion.EnemyID));
                                laserStillActive = false; // Laser is used up
                                break; // Stop checking this laser against other enemies
                            }
                        }
                        if (!laserStillActive) break; // If minion hit, stop checking this laser
                    }
                }
            }

            // Check enemy-to-player collisions
            CheckEnemyToPlayerCollisions(players, enemies, collisions);

            return collisions;
        }

        /// <summary>
        /// Check for enemy-to-player collisions. Players take damage when touching enemies.
        /// </summary>
        private void CheckEnemyToPlayerCollisions(List<Player> players, List<Enemy> enemies, List<Collision> collisions)
        {
            const int ENEMY_CONTACT_DAMAGE = 20; // Damage dealt by touching an enemy

            foreach (var player in players)
            {
                // Skip if player is inactive
                if (!player.Active)
                    continue;

                // Skip collision check if player is invincible
                if (player.IsInvincible())
                    continue;

                Rectangle playerRectangle = new Rectangle(
                    (int)player.Position.X,
                    (int)player.Position.Y,
                    player.Width,
                    player.Height);

                // Check collision with all parent enemies
                foreach (var enemy in enemies)
                {
                    if (!enemy.Active)
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
                        Console.WriteLine($"Player {player.PlayerName} collided with enemy - took {ENEMY_CONTACT_DAMAGE} damage");
                        break; // Only one enemy collision per frame per player
                    }

                    // Check minions
                    foreach (var minion in enemy.Minions)
                    {
                        if (!minion.Active)
                            continue;

                        Rectangle minionRectangle = new Rectangle(
                            (int)minion.Position.X,
                            (int)minion.Position.Y,
                            minion.Width,
                            minion.Height);

                        if (playerRectangle.Intersects(minionRectangle))
                        {
                            collisions.Add(new Collision(CollisionType.EnemyToPlayer, player.NetworkID, minion.EnemyID));
                            player.HandleEnemyCollision(ENEMY_CONTACT_DAMAGE);
                            Console.WriteLine($"Player {player.PlayerName} collided with minion - took {ENEMY_CONTACT_DAMAGE} damage");
                            break;
                        }
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
