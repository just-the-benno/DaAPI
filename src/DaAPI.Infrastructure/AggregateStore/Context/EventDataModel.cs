using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DaAPI.Infrastructure.AggregateStore.Context
{
    [Table("Events")]
    public class EventDataModel
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime UTCTimestamp { get; set; }

        [Required]
        public String StreamId { get; set; }

        [Required]
        public String EventType { get; set; }
        
        [Required]
        public String Content { get; set; }

        public Int64 Version { get; set; }

        public String MetaData { get; set; }

        public EventDataModel()
        {

        }

    }
}
