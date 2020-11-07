using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DaAPI.Infrastructure.StorageEngine
{
    [Table("Events")]
    public class EventEntry
    {
        [Key]
        public Int64 Id { get; set; }

        [Required]
        public String StreamId { get; set; }

        public Int32 Version { get; set; }

        [Required]
        public String BodyType { get; set; }

        [Required]
        public String Body { get; set; }

        public DateTime Timestamp { get; set; }

        public Int64 Position { get; set; }
    }
}
