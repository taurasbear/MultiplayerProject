﻿using MultiplayerProject.Source.GameObjects;

namespace MultiplayerProject.Source.Helpers.Factories
{
    public abstract class GameObjectFactory
    {
        public abstract GameObject CreateLaser();
        public abstract GameObject CreateExplosion();
    }
}
