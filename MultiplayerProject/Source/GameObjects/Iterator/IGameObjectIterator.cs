using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.GameObjects.Iterator
{
    /// <summary>
    /// Generic iterator interface for traversing collections of game objects
    /// </summary>
    /// <typeparam name="T">The type of object this iterator returns</typeparam>
    public interface IGameObjectIterator<T>
    {
        T GetNext();

        bool HasMore();
    }
}
