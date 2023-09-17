using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Models
{
    public class Entity
    {
        public int entity_name = 0;
        public string entity_type { get; set; }
        public int[] entity_coords { get; set; }
    }
}
