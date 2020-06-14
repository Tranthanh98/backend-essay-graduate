using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Services
{
    public class HandleDateTime
    {
        public static DateTime CreateDateTimeFromString(string dateTime)
        {
            return DateTime.Parse(dateTime);
        }
        public static double CalculateTimeStamp(DateTime startDate, DateTime nowDate)
        {
            return nowDate.Subtract(startDate).TotalSeconds;
        }
        public static double GetTimeStampNow()
        {
            var startDate = CreateDateTimeFromString("01/01/1970");
            var nowDate = CreateDateTimeFromString(DateTime.Now.ToString());

            return CalculateTimeStamp(startDate, nowDate);
        }
    }
}