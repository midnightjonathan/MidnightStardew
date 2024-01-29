using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidnightStardew.MidnightInteractions
{
    internal class MidnightStatement
    {
        public List<string> Statements { get; set; }
        public MidnightNpc? Speaker { get; set; }
        public int Image { get; set; }
        
        [JsonConstructor]
        public MidnightStatement(List<string> statements) 
        { 
            Statements = statements;
        }
    }
}
