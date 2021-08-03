using ExampleCode.DTOs;
using System.Collections.Generic;

namespace ExampleCode.Interfaces
{
    public interface IWorkedPeriods
    {
        public IWorkerInfoTimeTracking Worker { get; set; }
        public IEnumerable<Attendance> Attendances { get; set; }
    }
}
