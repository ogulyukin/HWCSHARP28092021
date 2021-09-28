using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlToJson
{
    public class RowElement
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public RowElement(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
