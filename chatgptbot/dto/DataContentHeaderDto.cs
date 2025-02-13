using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chatgptbot.dto
{
    public class DataContentHeaderDto
    {
        public string id { get; set; }
        public string thread_id { get; set; }
        public List<ContentDto> content { get; set; }

    }
}
