using System.Collections.Generic;
using Game.Data;

namespace Game.Services
{
    public interface IItemDatabase
    {
        ItemDefinition GetById(string id);
        IReadOnlyList<ItemDefinition> All { get; }
    }
}
