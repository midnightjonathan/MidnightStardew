using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightCharacters
{
    public class MidnightStats : Dictionary<string, int>
    {
        public new int this[string key]
        {
            get
            {
                if (TryGetValue(key.ToLower(), out int value))
                {
                    return value;
                }
                return 0;
            }
            set
            {
                base[key.ToLower()] = value;
            }
        }
    }
}
