using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.Entities
{
    public class Assistants
    {
        [Key]
        public int id { get; set; }
        public string assistantid { get; set; }
        public string faculty { get; set; }
    }
}
