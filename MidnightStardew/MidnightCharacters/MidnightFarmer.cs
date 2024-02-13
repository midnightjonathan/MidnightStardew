using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightCharacters
{
    public class MidnightFarmer
    {
        public static Farmer LocalFarmer
        {
            get
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer.IsLocalPlayer) return farmer;
                }
                throw new ApplicationException("No local farmer found.");
            }
        }
    }
}
