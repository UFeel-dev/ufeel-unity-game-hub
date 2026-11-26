using System.Collections.Generic;

namespace GameHub.Hub
{
    public sealed class DoorCategory
    {
        public DoorCategory(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public List<DoorIdentifier> Doors { get; } = new();
    }
}
