using ExampleCode.Interfaces;
using System;
using System.Collections.Generic;

namespace ExampleCode.DTOs
{
    //Это Класс DTO, уменьшил для примера
    public class WorkerInfo : IWorkerInfoTimeTracking
    {
        public string Fullname { get; set; }
        public string Position { get; set; }
        public string PersonalNumber { get; set; }

        //Много строк
    }

    public class Attendance
    {
        public DateTime Date { get; set; }
        public List<WorkerPeriod> Periods { get; set; }
    }

    public class WorkerPeriod
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
