using DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DaAPI.UnitTests.Helper
{
    public abstract class AggregateRootWithEventsTesterBase
    {
        protected T GetFirstEvent<T>(AggregateRootWithEvents root) where T : DomainEvent
        {
            IEnumerable<DomainEvent> events = root.GetChanges();
            Assert.Single(events);

            Assert.IsAssignableFrom<T>(events.First());

            return (T)events.First();
        }
    }
}
