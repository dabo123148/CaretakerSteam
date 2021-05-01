using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caretaker
{
    public enum Relationship
    {
        invalid = -1,
        unknown = 0,
        enemy = 1,
        neutral = 2,
        allied = 3,
        friendly = 4,
        tribemember = 5,
        beta = 6,
        //Amount of valid relationships
        max = 6
    }
}
