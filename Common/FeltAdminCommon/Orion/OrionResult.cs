using System;

using FeltAdmin.Database.API;
using FeltAdmin.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using FeltAdmin.Viewmodels;

namespace FeltAdmin.Orion
{
    public class OrionResult : IDatabaseObject
    {
        private string m_allSeries;

        private ResultType m_resultType;

        private bool m_doubleRange;

        public int OrionId { get; set; }

        public int Team { get; set; }

        public int Target { get; set; }

        public int ShooterId { get; set; }

        public string Name { get; set; }

        public string ClubName { get; set; }

        public string Class { get; set; }

        public int TotalSum { get; set; }

        public List<string> Series { get; set; }

        public bool DoubleRange
        {
            get
            {
                return this.m_doubleRange;
            }
            set
            {
                this.m_doubleRange = value;
            }
        }

        public ResultType ResultType
        {
            get
            {
                return this.m_resultType;
            }
            set
            {
                this.m_resultType = value;
            }
        }

        public int GetSum()
        {
            return this.GetSum(m_resultType);
        }

        public int GetInnerHits()
        {
            if (string.IsNullOrWhiteSpace(AllSeries))
            {
                return 0;
            }

            return AllSeries.Count(c => c == '*');
        }

        public int GetInnerHits(int range)
        {
            if (string.IsNullOrWhiteSpace(AllSeries))
            {
                return 0;
            }

            var seire = Series.First();
            if (range == 1)
            {
                var substring = string.Empty;
                if (seire.Length <= 6)
                {
                    substring = seire;
                }
                else
                {
                    substring = seire.Substring(0, 6);
                }

                return substring.Count(a => a == '*');
            }

            if (range == 2)
            {
                var substring = string.Empty;
                if (seire.Length <= 6)
                {
                    return 0;
                }
                else
                {
                    substring = seire.Substring(6, seire.Length - 6);
                }

                return substring.Count(a => a == '*');
            }

            return this.GetInnerHits();
        }

        public int GetSum(ResultType resultType)
        {
            int sum = 0;
            if (Series == null || !Series.Any())
            {
                return 0;
            }

            foreach (var serie in Series)
            {
                sum += GetValue(resultType, serie);
            }

            return sum;
        }


        public int GetSum(ResultType resultType, int range)
        {
            int sum = 0;
            if (Series == null || !Series.Any())
            {
                return 0;
            }

            foreach (var serie in Series)
            {
                sum += GetValue(resultType, serie, range);
            }

            return sum;
        }
        
        public List<string> GetPrintableSeries()
        {
            if (m_resultType == ResultType.Bane)
            {
                return Series;
            }

            var result = new List<string>();
            foreach (var serie in Series)
            {
                var sum = GetValue(m_resultType, serie);
                var innerhits = serie.Count(s => s == '*');

                result.Add(string.Format("{0}/{1}", sum, innerhits));
            }

            return result;
        }

        private static int GetValue(ResultType resultType, string rawserie, int range = 0)
        {
            var serie = rawserie.Trim();
            int sum = 0;
            if (!string.IsNullOrWhiteSpace(serie))
            {
                int startidx = 0;
                int length = serie.Length;

                if (range == 1)
                {
                    length = Math.Min(6, serie.Length);
                }

                if (range == 2)
                {
                    if (serie.Length <= 6)
                    {
                        return 0;
                    }

                    startidx = 6;
                    length = serie.Length - 6;
                }

                for (int idx = startidx; idx < startidx + length; idx++)
                {
                    switch (serie[idx])
                    {
                        case '0':
                            break;
                        case '1':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 1;
                            }
                            break;
                        case '2':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 2;
                            }
                            break;
                        case '3':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 3;
                            }
                            break;
                        case '4':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 4;
                            }
                            break;
                        case '5':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 5;
                            }
                            break;
                        case '6':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 6;
                            }
                            break;
                        case '7':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 7;
                            }
                            break;
                        case '8':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 8;
                            }
                            break;
                        case '9':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 9;
                            }
                            break;
                        case 'X':
                        case 'x':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 10;
                            }
                            else
                            {
                                sum += 1;
                            }
                            break;
                        case '*':
                            if (resultType == ResultType.Bane)
                            {
                                sum += 10;
                            }
                            else
                            {
                                sum += 1;
                            }
                            break;
                        default:
                            Log.Error(string.Format("Unknown value: {0}", serie[idx]));
                            break;
                    }
                }
                ////foreach (var shot in serie.Trim())
                ////{
                ////    switch (shot)
                ////    {
                ////        case '0':
                ////            break;
                ////        case '1':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 1;
                ////            }
                ////            break;
                ////        case '2':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 2;
                ////            }
                ////            break;
                ////        case '3':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 3;
                ////            }
                ////            break;
                ////        case '4':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 4;
                ////            }
                ////            break;
                ////        case '5':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 5;
                ////            }
                ////            break;
                ////        case '6':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 6;
                ////            }
                ////            break;
                ////        case '7':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 7;
                ////            }
                ////            break;
                ////        case '8':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 8;
                ////            }
                ////            break;
                ////        case '9':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 9;
                ////            }
                ////            break;
                ////        case 'X':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 10;
                ////            }
                ////            else
                ////            {
                ////                sum += 1;
                ////            }
                ////            break;
                ////        case '*':
                ////            if (resultType == ResultType.Bane)
                ////            {
                ////                sum += 10;
                ////            }
                ////            else
                ////            {
                ////                sum += 1;
                ////            }
                ////            break;
                ////        default:
                ////            Log.Error(string.Format("Unknown value: {0}", shot));
                ////            break;
                ////    }
                ////}
            }

            return sum;
        }

        public string AllSeries
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_allSeries) && Series.Any())
                {
                    m_allSeries = string.Join(";", Series);
                }

                return this.m_allSeries;
            }
            set
            {
                this.m_allSeries = value;
                if (!string.IsNullOrWhiteSpace(m_allSeries) && (Series == null || !Series.Any()))
                {
                    Series = ParseSeries(m_allSeries);
                }
            }
        }

        public static OrionResult ParseFromDatabase(string keyvaluePairs)
        {
            var result = new OrionResult();
            var tuples = keyvaluePairs.Split(';');
            foreach (var tuple in tuples)
            {
                var keyvaluePair = tuple.Split('=');
                switch (keyvaluePair[0].Trim())
                {
                    case "OrionId":
                        result.OrionId = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "Team":
                        result.Team = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "Target":
                        result.Target = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "ShooterId":
                        result.ShooterId = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "Name":
                        result.Name = keyvaluePair[1].Trim();
                        break;
                    case "ClubName":
                        result.ClubName = keyvaluePair[1].Trim();
                        break;
                    case "Class":
                        result.Class = keyvaluePair[1].Trim();
                        break;
                    case "TotalSum":
                        result.TotalSum = int.Parse(keyvaluePair[1].Trim());
                        break;
                    case "AllSeries":
                        result.AllSeries = keyvaluePair[1].Trim();
                        break;
                }
            }

            return result;

        }

        public static OrionResult ParseFromOrion(string orionResultLine)
        {
            if (string.IsNullOrWhiteSpace(orionResultLine))
            {
                Log.Error("Empty orion result line");
                return null;
            }

            var tokens = orionResultLine.Split(';');
            var result = new OrionResult
                         {
                             OrionId = TryGetIntFromToken(tokens[0], "OrionId"),
                             Team = TryGetIntFromToken(tokens[1], "Team"),
                             Target = TryGetIntFromToken(tokens[2], "Target"),
                             ShooterId = TryGetIntFromToken(tokens[3], "ShooterId"),
                             Name = tokens[4],
                             ClubName = tokens[5],
                             Class = tokens[6],
                             TotalSum = TryGetIntFromToken(tokens[7], "TotalSum")
                         };

            if (tokens.Length > 8)
            {
                result.Series = new List<string>();

                for (int c = 8; c < tokens.Length; c++)
                {
                    result.Series.Add(tokens[c]);
                }
            }

            result.AllSeries = string.Join(";", result.Series);
            return result;
        }

        private static List<string> ParseSeries(string series)
        {
            var result = new List<string>();
            var tokens = series.Split(';');
            if (tokens.Any())
            {
                foreach (var token in tokens)
                {
                    result.Add(token);
                }
            }

            return result;
        }

        private static int TryGetIntFromToken(string token, string parameterName)
        {
            int result;
            if (int.TryParse(token, out result))
            {
                return result;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                Log.Error(string.Format("Not able to parse to integer: {0}. Parametername={1}", token, parameterName));
            }

            return 0;
        }

        public TableName TableName
        {
            get { return TableName.OrionResult; }
        }

        public Dictionary<int, ColumnInfo> RowInfo
        {
            get
            {
                var row = new Dictionary<int, ColumnInfo>();
                row.Add(0, new ColumnInfo { ColumnName = "OrionId", ColumnValue = OrionId });
                row.Add(1, new ColumnInfo { ColumnName = "Team", ColumnValue = Team });
                row.Add(2, new ColumnInfo { ColumnName = "Target", ColumnValue = Target });
                row.Add(3, new ColumnInfo { ColumnName = "ShooterId", ColumnValue = ShooterId });
                row.Add(4, new ColumnInfo { ColumnName = "Name", ColumnValue = Name });
                row.Add(5, new ColumnInfo { ColumnName = "ClubName", ColumnValue = ClubName });
                row.Add(6, new ColumnInfo { ColumnName = "Class", ColumnValue = Class });
                row.Add(7, new ColumnInfo { ColumnName = "TotalSum", ColumnValue = TotalSum });
                row.Add(8, new ColumnInfo { ColumnName = "AllSeries", ColumnValue = AllSeries });

                return row;
            }
        }
    }
}
