// File: EnemyCommands.cs
// Location: MultiplayerProject/Source/Interpreter/Commands/EnemyCommands.cs

using Microsoft.Xna.Framework;
using System.Reflection;

namespace MultiplayerProject.Source.Commands
{
    /// <summary>
    /// Unified command for all enemy-related operations
    /// </summary>
    public class EnemyCommand : ICommandExpression
    {
        public enum EnemyAction
        {
            Spawn,
            Clear, 
            SetSpawnRate
        }

        private readonly EnemyAction _action;
        private readonly EnemyType _enemyType;
        private readonly float _x, _y;
        private readonly float _spawnRate;

        // Constructor for spawn action
        public EnemyCommand(EnemyAction action, EnemyType enemyType, float x, float y) 
        {
            _action = action;
            _enemyType = enemyType;
            _x = x;
            _y = y;
        }

        // Constructor for spawn rate action
        public EnemyCommand(EnemyAction action, float spawnRate)
        {
            _action = action;
            _spawnRate = spawnRate;
        }

        // Constructor for clear action
        public EnemyCommand(EnemyAction action)
        {
            _action = action;
        }

        public string Interpret(GameCommandContext context)
        {
            if (context.CurrentGameInstance == null)
            {
                return "Error: No active game instance. Cannot perform enemy operations.";
            }

            switch (_action)
            {
                case EnemyAction.Spawn:
                    return SpawnEnemy(context);
                case EnemyAction.Clear:
                    return ClearEnemies(context);
                case EnemyAction.SetSpawnRate:
                    return SetSpawnRate(context);
                default:
                    return "Error: Unknown enemy action.";
            }
        }

        private string SpawnEnemy(GameCommandContext context)
        {
            try
            {
                var gameFacade = GetPrivateField(context.CurrentGameInstance, "_gameFacade");
                if (gameFacade != null)
                {
                    var setEnemyTypeMethod = gameFacade.GetType().GetMethod("SetNextEnemyType");
                    setEnemyTypeMethod?.Invoke(gameFacade, new object[] { _enemyType });
                    
                    var addEnemyMethod = gameFacade.GetType().GetMethod("AddNewEnemy");
                    var enemy = addEnemyMethod?.Invoke(gameFacade, null);
                    
                    if (enemy != null)
                    {
                        SetEnemyPosition(enemy, _x, _y);
                        UpdateLifetimeStatistics(context, enemy);
                        
                        var enemyId = GetPropertyValue(enemy, "EnemyID")?.ToString() ?? System.Guid.NewGuid().ToString();
                        return $"Enemy '{_enemyType}' spawned at ({_x}, {_y}). ID: {enemyId}";
                    }
                }
                return $"Enemy spawn attempted: {_enemyType} at ({_x}, {_y})";
            }
            catch (System.Exception ex)
            {
                return $"Error spawning enemy: {ex.Message}";
            }
        }

        private string ClearEnemies(GameCommandContext context)
        {
            try
            {
                var gameFacade = GetPrivateField(context.CurrentGameInstance, "_gameFacade");
                var enemyManager = GetEnemyManager(gameFacade);
                var enemies = GetEnemiesList(enemyManager);
                
                if (enemies != null)
                {
                    int count = enemies.Count;
                    enemies.Clear();
                    return $"Cleared {count} enemies from the game.";
                }
                return "Enemy clear attempted but access failed.";
            }
            catch (System.Exception ex)
            {
                return $"Error clearing enemies: {ex.Message}";
            }
        }

        private string SetSpawnRate(GameCommandContext context)
        {
            if (_spawnRate <= 0)
                return "Error: Spawn rate must be greater than 0 seconds.";

            try
            {
                var field = context.CurrentGameInstance.GetType()
                    .GetField("_enemySpawnTime", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (field != null)
                {
                    field.SetValue(context.CurrentGameInstance, System.TimeSpan.FromSeconds(_spawnRate));
                    context.SetVariable("enemy_spawn_rate", _spawnRate);
                    return $"Enemy spawn rate set to {_spawnRate} seconds.";
                }
                
                context.SetVariable("enemy_spawn_rate", _spawnRate);
                return $"Enemy spawn rate set to {_spawnRate} seconds (context only).";
            }
            catch (System.Exception ex)
            {
                return $"Error setting spawn rate: {ex.Message}";
            }
        }

        // Helper methods
        private void SetEnemyPosition(object enemy, float x, float y)
        {
            var position = new Vector2(x, y);
            var positionProperty = enemy.GetType().GetProperty("Position");
            var positionField = enemy.GetType().GetField("Position");
            
            positionProperty?.SetValue(enemy, position);
            positionField?.SetValue(enemy, position);
        }

        private void UpdateLifetimeStatistics(GameCommandContext context, object enemy)
        {
            var lifetimeVisitor = GetPrivateField(context.CurrentGameInstance, "_lifetimeStatsVisitor");
            if (lifetimeVisitor != null)
            {
                var visitMethod = lifetimeVisitor.GetType().GetMethod("Visit", new[] { enemy.GetType() });
                visitMethod?.Invoke(lifetimeVisitor, new[] { enemy });
            }
        }

        private object GetEnemyManager(object gameFacade)
        {
            return gameFacade?.GetType().GetProperty("EnemyManager")?.GetValue(gameFacade);
        }

        private System.Collections.IList GetEnemiesList(object enemyManager)
        {
            return enemyManager?.GetType().GetProperty("Enemies")?.GetValue(enemyManager) as System.Collections.IList;
        }

        private object GetPrivateField(object obj, string fieldName)
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj);
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null) return property.GetValue(obj);
            
            var field = obj.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return field?.GetValue(obj);
        }
    }
}