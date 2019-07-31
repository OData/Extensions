using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataDemo
{
    internal class SimpleProduct
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime DiscontinuedDate { get; set; }
        public int Rating { get; set; }
        public long Price { get; set; }
        public SimpleProduct()
        {

        }
    }
}
