using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine
{
    [Table("Helper")]
    public class HelperEntry
    {
        [Key]
        public String Name { get; set; }

        [Required]
        public String Content { get; set; }
    }
}
