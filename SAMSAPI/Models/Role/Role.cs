using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Role()
        { }
    }
}