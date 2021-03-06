﻿using NExtends.Primitives.DateTimes;
using System;
using System.Collections.Generic;

namespace RDD.Domain.Models
{
    public class Period
    {
        public Period(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }

        public TimeSpan Duration
        {
            get => End - Start;
            set => End = Start.Add(value);
        }

        public DateTime End { get; set; }

        public DateTime StartsAt
        {
            get => Start;
            set => Start = value;
        }

        public DateTime EndsAt
        {
            get => End;
            set => End = value;
        }

        /// <summary>
        /// Calculates the period (Start and End dates) for a given date, based on the displayMode parameter
        /// </summary>
        /// <param name="mode">Mode</param>
        /// <param name="dt">Any date (used to calculate the period)</param>
        /// <returns>Period which includes dt</returns>
        public static Period GetPeriodDates(Mode mode, DateTime dt)
        {
            DateTime dtStart, dtEnd;

            switch (mode)
            {
                case Mode.Quizaine:
                    if (dt.Day <= 15)
                    {
                        dtStart = new DateTime(dt.Year, dt.Month, 1);
                        dtEnd = new DateTime(dt.Year, dt.Month, 15);
                    }
                    else
                    {
                        dtStart = new DateTime(dt.Year, dt.Month, 16);
                        dtEnd = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
                    }
                    break;

                case Mode.Mois:
                    dtStart = new DateTime(dt.Year, dt.Month, 1);
                    dtEnd = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
                    break;

                default:
                    dtStart = dt.PreviousOrCurrent(DayOfWeek.Monday);
                    dtEnd = dt.NextOrCurrent(DayOfWeek.Sunday);
                    break;
            }

            return new Period(dtStart, dtEnd);
        }

        public static List<Period> GetPastDateRanges(Mode mode, DateTime dtStart, int nbPeriod, bool includeDtStart)
        {
            // we want to include dtStart => add one day
            DateTime prevDtStart = includeDtStart ? dtStart.AddDays(1) : dtStart;

            var dateRanges = new List<Period>();

            for (var i = 0; i < nbPeriod; i++)
            {
                // Date we look => last date of previous range
                Period period = GetPeriodDates(mode, prevDtStart.AddDays(-1));

                dateRanges.Add(new Period(period.Start, period.End));

                // for next iteration
                prevDtStart = period.Start;
            }

            return dateRanges;
        }

        /// <summary>
        /// Permet de passer les parametres de type Period (between, until, etc) pour les widgets qui ne passent pas encore par l'API
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static Period Parse(string dateQueryStringKey, string dateQueryStringValue)
        {
            try
            {
                //Ex : date=until,2011-09-13
                if (dateQueryStringValue.Contains("until"))
                {
                    return new Period((DateTime) System.Data.SqlTypes.SqlDateTime.MinValue, Convert.ToDateTime(dateQueryStringValue.Split(',')[1]));
                }
                //Ex : date=since,2011-09-13
                if (dateQueryStringValue.Contains("since"))
                {
                    return new Period(Convert.ToDateTime(dateQueryStringValue.Split(',')[1]), (DateTime) System.Data.SqlTypes.SqlDateTime.MaxValue);
                }
                //Ex : date=between,2011-09-01,2011-09-30
                if (dateQueryStringValue.Contains("between"))
                {
                    return new Period(Convert.ToDateTime(dateQueryStringValue.Split(',')[1]), Convert.ToDateTime(dateQueryStringValue.Split(',')[2]));
                }
                //On considède que c'est une date seule : date=2011-09-01, on crée le range 2011-09-01T00:00:00.000 à 2011-09-01T23:59:59.999
                return new Period(Convert.ToDateTime(dateQueryStringValue), Convert.ToDateTime(dateQueryStringValue).AddDays(1).AddMilliseconds(-2)); //-2 car en SQL, 23:59:59:999 == 00:00:00:000 le lendemain !
            }
            catch
            {
                throw new Exception("The parameter '" + dateQueryStringKey + "' does not respect DateTime parameters format");
            }
        }

        public enum Mode
        {
            Semaine,
            Quizaine,
            Mois
        }
    }
}