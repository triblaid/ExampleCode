using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Text.Json.Serialization;
using ExampleCode.Interfaces;

namespace ExampleCode.DTOs
{
    public class TimeTrackingReportDTO
    {
        private TimeSpan hoursInFirstHalf => TimeSpan.FromSeconds(Attendance.Where(x => x.DayNumber < 16).Sum(x => x.WorkedDayTime.TotalSeconds + x.WorkedNightTime.TotalSeconds));
        private TimeSpan hoursInSecondHalf => TimeSpan.FromSeconds(Attendance.Where(x => x.DayNumber >= 16).Sum(x => x.WorkedDayTime.TotalSeconds + x.WorkedNightTime.TotalSeconds));

        public string HoursInFirstHalf => string.Format("{0:00}:{1:00}", hoursInFirstHalf.Days * 24 + hoursInFirstHalf.Hours, hoursInFirstHalf.Minutes);
        public string HoursInSecondHalf => string.Format("{0:00}:{1:00}", hoursInSecondHalf.Days * 24 + hoursInSecondHalf.Hours, hoursInSecondHalf.Minutes);
        public int TotalWeekend => Attendance.Where(x => x.TimeTrackingType == TimeTrackingType.Weekend).Count();
        public WorkedTotal WorkedInTotal => new WorkedTotal(Attendance);
        public Absent WasAbsent => new Absent(Attendance);
        public List<WorkedDayOfMonth> Attendance { get; set; } = new List<WorkedDayOfMonth>();
        public IWorkerInfoTimeTracking Worker { get; set; }

        public TimeTrackingReportDTO(IWorkedPeriods workedPeriods, DateTime begin, DateTime end)
        {
            Worker = workedPeriods.Worker;

            DateTime date = begin;
            while (date < end)
            {
                var nextDate = date.AddDays(1);
                var attendance = workedPeriods.Attendances.FirstOrDefault(x => x.Date.Date == date.Date);
                if (attendance != null)
                    Attendance.Add(new WorkedDayOfMonth(attendance.Periods, date.Date));
                else
                    Attendance.Add(new WorkedDayOfMonth(date.Date));

                date = nextDate;
            }
        }
    }

    public class WorkedTotal
    {
        private TimeSpan _totalHours => _daytime + _nightTime + _weekend;
        private TimeSpan _daytime { get; set; }
        private TimeSpan _nightTime { get; set; }
        private TimeSpan _weekend { get; set; }

        public int TotalDays { get; set; }
        public string DaytimeHours => string.Format("{0:00}:{1:00}", _daytime.Days * 24 + _daytime.Hours, _daytime.Minutes);
        public string NightTimeHours => string.Format("{0:00}:{1:00}", _nightTime.Days * 24 + _nightTime.Hours, _nightTime.Minutes);
        public string WeekendHours => string.Format("{0:00}:{1:00}", _weekend.Days * 24 + _weekend.Hours, _weekend.Minutes);
        public string TotalHours => string.Format("{0:00}:{1:00}", _totalHours.Days * 24 + _totalHours.Hours, _totalHours.Minutes);

        public WorkedTotal(List<WorkedDayOfMonth> attendance)
        {
            TotalDays = attendance.Where(x => x.TimeTrackingType != TimeTrackingType.Absence).Count();
            _daytime = TimeSpan.FromSeconds(attendance.Where(x => x.TimeTrackingType != TimeTrackingType.WorkWeekend).Sum(x => x.WorkedDayTime.TotalSeconds));
            _nightTime = TimeSpan.FromSeconds(attendance.Where(x => x.TimeTrackingType != TimeTrackingType.WorkWeekend).Sum(x => x.WorkedNightTime.TotalSeconds));
            _weekend = TimeSpan.FromSeconds(attendance.Where(x => x.TimeTrackingType == TimeTrackingType.WorkWeekend).Sum(x => x.WorkedDayTime.TotalSeconds + x.WorkedNightTime.TotalSeconds));
        }
    }

    public class Absent
    {
        public int TotalDays { get; set; }
        public int TotalHours => TotalDays * 8;

        public Absent(List<WorkedDayOfMonth> attendance)
        {
            TotalDays = attendance.Where(x => x.TimeTrackingType == TimeTrackingType.Absence).Count();
        }
    }

    public class WorkedDayOfMonth
    {
        private TimeSpan _startTime { get; set; }
        private TimeSpan _endTime { get; set; }
        [JsonIgnore]
        public int DayNumber { get; set; }
        [JsonIgnore]
        public TimeTrackingType TimeTrackingType { get; set; }
        [JsonIgnore]
        public TimeSpan WorkedDayTime { get; set; }
        [JsonIgnore]
        public TimeSpan WorkedNightTime { get; set; }
        public string Start => string.Format("{0:00}:{1:00}", _startTime.Hours, _startTime.Minutes);
        public string End => string.Format("{0:00}:{1:00}", _endTime.Hours, _endTime.Minutes);
        /// <summary>
        /// Тип смены для фронта
        /// </summary>
        public string Kind => Utils.Description.GetDescription(TimeTrackingType);
        /// <summary>
        /// Отработанное время для отображения
        /// </summary>
        public string Duration
        {
            get
            {
                StringBuilder workedTime = new StringBuilder();
                if (WorkedDayTime != new TimeSpan())
                    workedTime.Append(string.Format("{0:00}:{1:00}", WorkedDayTime.Hours, WorkedDayTime.Minutes));
                if (WorkedDayTime != new TimeSpan() && WorkedNightTime != new TimeSpan())
                    workedTime.Append("/");
                if (WorkedNightTime != new TimeSpan())
                    workedTime.Append(string.Format("{0:00}:{1:00}", WorkedNightTime.Hours, WorkedNightTime.Minutes));
                if (WorkedDayTime == new TimeSpan() && WorkedNightTime == new TimeSpan())
                    workedTime.Append("00:00");

                return workedTime.ToString();
            }
        }

        public WorkedDayOfMonth(List<WorkerPeriod> periods, DateTime dateTime)
        {
            _startTime = periods.Count() != 0 ? periods.OrderBy(x => x.Start).FirstOrDefault().Start : new TimeSpan();
            _endTime = periods.Count() != 0 ? periods.OrderByDescending(x => x.End).FirstOrDefault().End : new TimeSpan();
            DayNumber = dateTime.Day;
            WorkedDayTime = new TimeSpan();
            WorkedNightTime = new TimeSpan();

            //Заполенение рабочего времени
            var startNightShift = new TimeSpan(22, 00, 00); // Начало ночного времени
            var endNightShift = new TimeSpan(6, 00, 00); // Конец ночного вермени
            foreach (var period in periods)
            {
                //Проверка на ночное рабочее время
                var _workedNightTime = new TimeSpan();
                if (period.Start < endNightShift && period.End >= endNightShift) // проверка на то что смена была 00:00 - 21:59
                    _workedNightTime += endNightShift - period.Start;

                if (period.End <= endNightShift && period.Start <= endNightShift)
                    _workedNightTime += period.End - period.Start;

                if (period.Start < startNightShift && period.End >= startNightShift)
                    _workedNightTime += period.End - startNightShift;

                if (period.End >= startNightShift && period.Start > startNightShift)
                    _workedNightTime += period.End - period.Start;

                WorkedNightTime += _workedNightTime;
                WorkedDayTime += period.End - period.Start - _workedNightTime;
            }

            if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday) // Выходные ли это
                if (WorkedDayTime + WorkedNightTime > new TimeSpan()) // работал в выходные дни?
                    TimeTrackingType = TimeTrackingType.WorkWeekend;
                else
                    TimeTrackingType = TimeTrackingType.Weekend;
            else if (WorkedDayTime > new TimeSpan() && WorkedNightTime > new TimeSpan()) // Провекра была ли ночная смена вместе с дневной
                TimeTrackingType = TimeTrackingType.DayAndNight;
            else if (WorkedDayTime > new TimeSpan()) // Была ли только дневная смена
                TimeTrackingType = TimeTrackingType.DayShift;
            else if (WorkedNightTime > new TimeSpan()) // Была ли только ночная смена
                TimeTrackingType = TimeTrackingType.NightShift;
            else
                TimeTrackingType = TimeTrackingType.Absence; // Сотрудник не вышел на работу
        }

        public WorkedDayOfMonth(DateTime dateTime)
        {
            _startTime = new TimeSpan();
            _endTime = new TimeSpan();
            DayNumber = dateTime.Day;
            WorkedDayTime = new TimeSpan();
            WorkedNightTime = new TimeSpan();
            TimeTrackingType = dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday ? TimeTrackingType.Weekend : TimeTrackingType.Absence;
        }
    }

    public enum TimeTrackingType
    {
        [Description("Я")]
        DayShift = 0,
        [Description("Н")]
        NightShift = 1,
        [Description("Я/Н")]
        DayAndNight = 2,
        [Description("В")]
        Weekend = 3,
        [Description("РВ")]
        WorkWeekend = 4,
        [Description("НН")]
        Absence = 5
    }

    public enum TimeTrackingTypeBuild
    {
        [Description("Отчет из базы данных")]
        ByDB = 0,
        [Description("Отчет из 1С")]
        By1C = 1,
        [Description("Отчет из Excel")]
        ByExcel = 2,
    }
}
