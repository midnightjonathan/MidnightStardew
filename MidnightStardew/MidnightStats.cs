using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew
{
    public class MidnightStats : Dictionary<string, int>
    {
        public new int this[string key]
        {
            get
            {
                if (base.TryGetValue(key.ToLower(), out int value))
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
