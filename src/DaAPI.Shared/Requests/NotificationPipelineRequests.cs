using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Shared.Requests
{
    public static class NotificationPipelineRequests
    {
        public static class V1
        {
            public class CreateNotifcationPipelineRequest
            {
                [Required]
                [StringLength(100, MinimumLength = 3)]
                public String Name { get; set; }

                [StringLength(500)]
                public String Description { get; set; }
                
                [Required]
                public String TriggerName { get; set; }

                public String CondtionName { get; set; }
                public IDictionary<String, String> ConditionProperties { get; set; }

                [Required]
                public String ActorName { get; set; }
                
                [Required]
                public IDictionary<String, String> ActorProperties { get; set; }
            }
        }
    }
}
