﻿using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public abstract class GameObjectFactory
    {
        public abstract GameObject GetLaser();
        public abstract GameObject GetExplosion();
    }
}
