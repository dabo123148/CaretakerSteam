using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caretaker
{
    public class SteamIDData
    {
        public ulong SteamID;
        public TribeData tribe;
        public string name;
        public Relationship relation = Relationship.invalid;
        public bool HasData()
        {
            if (HasTribe() || HasName() || HasRelation()) return true;
            return false;
        }
        public bool HasName()
        {
            if (name != null && name.Length != 0) return true;
            return false;
        }
        public bool HasTribe()
        {
            if (tribe != null) return true;
            return false;
        }
        public bool HasRelation()
        {
            if (relation.CompareTo(Relationship.invalid) != 0) return true;
            return false;
        }
    }
}
