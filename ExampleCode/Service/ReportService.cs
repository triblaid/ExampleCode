using ExampleCode.DTOs;
using ExampleCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExampleCode.Service
{
    public class ReportService
    {
        delegate IEnumerable<IWorkedPeriods> ProcessingWorkedPeriods(string objectId, DateTime begin, DateTime end);
        public List<TimeTrackingReportDTO> GetTimeTrackingReport(string objectId, DateTime begin, DateTime end, TimeTrackingTypeBuild typeBuild)
        {
            ProcessingWorkedPeriods processing = СhooseTypeBuild(objectId, begin, end ,typeBuild);

            var workedPeriods = processing(objectId, begin, end);

            var viewModels = workedPeriods.Select(workedPeriod => new TimeTrackingReportDTO(workedPeriod, begin, end));

            return viewModels.ToList();
        }

        private ProcessingWorkedPeriods СhooseTypeBuild(string objectId, DateTime begin, DateTime end, TimeTrackingTypeBuild typeBuild) =>
        typeBuild switch
        {
            TimeTrackingTypeBuild.By1C => GetWorkedPeriodBy1C,
            TimeTrackingTypeBuild.ByDB => GetWorkedPeriodByDB,
            TimeTrackingTypeBuild.ByExcel => GetWorkedPeriodByExcel,
            _ => throw new ArgumentException("Ошибка типа постронения отчета \"Учет рабочего времени\" ")
        };

        private IEnumerable<IWorkedPeriods> GetWorkedPeriodByDB(string objectId, DateTime begin, DateTime end)
        {
            return null;
        }

        private IEnumerable<IWorkedPeriods> GetWorkedPeriodBy1C(string objectId, DateTime begin, DateTime end)
        {
            return null;
        }

        private IEnumerable<IWorkedPeriods> GetWorkedPeriodByExcel(string objectId, DateTime begin, DateTime end)
        {
            return null;
        }
    }
}
