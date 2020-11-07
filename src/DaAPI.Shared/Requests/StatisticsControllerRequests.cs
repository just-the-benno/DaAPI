using DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DaAPI.Shared.Requests
{
    public static class StatisticsControllerRequests
    {
        public static class V1
        {
            public enum GroupStatisticsResultBy
            {
                Day = 1,
                Week = 2,
                Month = 3
            }

            public class TimeSeriesFilterRequest : IValidatableObject
            {
                public DateTime? Start { get; set; }
                public DateTime? End { get; set; }

                public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
                {
                    if (Start.HasValue == true && End.HasValue == true && Start.Value > End.Value)
                    {
                        yield return new ValidationResult("Start needs to be smaller than end", new[] { nameof(Start), nameof(End) });
                    }
                }
            }

            public class GroupedTimeSeriesFilterRequest : TimeSeriesFilterRequest
            {
                public GroupStatisticsResultBy GroupbBy { get; set; }
            }

            public class DHCPv6PacketTypeBasedTimeSeriesFilterRequest : TimeSeriesFilterRequest
            {
                public DHCPv6PacketTypes PacketType { get; set; }
            }
        }
    }
}
