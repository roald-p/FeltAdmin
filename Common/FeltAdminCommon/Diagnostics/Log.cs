using System;
using System.Collections.Generic;
using System.Globalization;

namespace FeltAdmin.Diagnostics
{
    public static class Log
    {
        static readonly List<ILogAppender> s_appenders = new List<ILogAppender>();
        static readonly ILogAppender s_failoverAppender = new TraceAppender(LoggingLevels.Trace);

        public static ILogAppender Failover
        {
            get { return s_failoverAppender; }
        }

        /// <summary>
        /// Adds a log appender to the logger.
        /// </summary>
        /// <param name="appender">The appender to be added.</param>
        public static void AddAppender(ILogAppender appender)
        {
            if (appender == null)
            {
                throw new ArgumentNullException("appender");
            }

            s_appenders.Add(appender);
        }

        /// <summary>
        /// Clears all log appenders from the logger.
        /// </summary>
        public static void RemoveAppenders()
        {
            if (s_appenders != null)
            {
                s_appenders.Clear();
            }
        }

        #region internal
        /// <summary>
        /// Gets a previously added appender by index. Used for testing.
        /// </summary>
        /// <param name="index">Index of appender.</param>
        /// <returns>ILogAppender that has been added using AddAppender.</returns>
        internal static ILogAppender GetAppender(int index)
        {
            return s_appenders[index];
        }

        /// <summary>
        /// Returns the count of appenders that have been added using AddAppender.
        /// </summary>
        internal static int AppenderCount
        {
            get { return s_appenders.Count; }
        }
        #endregion

        #region private

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">Current logging level</param>
        /// <param name="exception">The exception that should be logged</param>
        /// <returns>Guid that identifies the error incident.</returns>
        private static Guid LogWrite(LoggingLevels level, LoggingEvent loggingEvent)
        {
            Guid incidentId = Guid.Empty;
           
            if (s_appenders != null && s_appenders.Count > 0)
            {

                foreach (ILogAppender appender in s_appenders)
                {
                    try
                    {
                        if (appender != null &&
                            appender.LoggingLevel != LoggingLevels.None &&
                            appender.LoggingLevel >= level)
                        {
                            appender.Append(level, loggingEvent);
                        }
                    }
                    catch
                    {
                        //Do nothing...
                    }
                }

            }
            else
            {
                Log.Failover.Append(level, loggingEvent);
            }
            return incidentId;
        }

        #endregion

        #region Error

        /// <summary>
        /// Logs an error with the specified description.
        /// </summary>
        /// <param name="loggingEvent">The logging event</param>
        /// <returns>Guid that identifies the error incident.</returns>
        public static Guid Error(LoggingEvent loggingEvent)
        {
            return Log.LogWrite(LoggingLevels.Error, loggingEvent);
        }

        /// <summary>
        /// Logs an error with the specified description.
        /// </summary>
        /// <param name="description">The description</param>
        public static void Error(string description)
        {
            Log.Error(null, description);
        }

        /// <summary>
        /// Logs an error with the specified description.
        /// </summary>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Error(string format, params object[] args)
        {
            Log.Error(null, format, args);
        }

        /// <summary>
        /// Logs an error with the specified exception and description.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="description">The description</param>
        /// <returns>Guid that identifies the error incident.</returns>
        public static Guid Error(Exception exception, string description)
        {
            return Log.Error(new LoggingEvent(description, exception));
        }

        /// <summary>
        /// Logs an error with the specified exception and description.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        /// <returns>Guid that identifies the error incident.</returns>
        public static Guid Error(Exception exception, string format, params object[] args)
        {
            return Log.Error(exception, String.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion

        #region Info

        /// <summary>
        /// Logs an information description with the specified logging event.
        /// </summary>
        /// <param name="loggingEvent">The logging event</param>
        public static void Info(LoggingEvent loggingEvent)
        {
            Log.LogWrite(LoggingLevels.Info, loggingEvent);
        }

        /// <summary>
        /// Logs a information description with the specified string.
        /// </summary>
        /// <param name="description">The description</param>
        public static void Info(string description)
        {
            Log.Info(null, description);
        }

        /// <summary>
        /// Logs a information description with the specified string.
        /// </summary>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Info(string format, params object[] args)
        {
            Log.Info(null, format, args);
        }

        /// <summary>
        /// Logs a information description with the specified exception and string.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="description">The description</param>
        public static void Info(Exception exception, string description)
        {
            Log.Info(new LoggingEvent(description, exception));
        }

        /// <summary>
        /// Logs a information description with the specified exception and string.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Info(Exception exception, string format, params object[] args)
        {
            Log.Info(exception, String.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion

        #region Warning

        /// <summary>
        /// Logs an information description with the specified logging event.
        /// </summary>
        /// <param name="loggingEvent">The logging event</param>
        public static void Warning(LoggingEvent loggingEvent)
        {
            Log.LogWrite(LoggingLevels.Warning, loggingEvent);
        }

        /// <summary>
        /// Logs a information description with the specified string.
        /// </summary>
        /// <param name="description">The description</param>
        public static void Warning(string description)
        {
            Log.Warning(null, description);
        }

        /// <summary>
        /// Logs a information description with the specified string.
        /// </summary>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Warning(string format, params object[] args)
        {
            Log.Warning(null, format, args);
        }

        /// <summary>
        /// Logs a information description with the specified exception and string.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="description">The description</param>
        public static void Warning(Exception exception, string description)
        {
            Log.Warning(new LoggingEvent(description, exception));
        }

        /// <summary>
        /// Logs a information description with the specified exception and string.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Warning(Exception exception, string format, params object[] args)
        {
            Log.Warning(exception, String.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion

        #region Trace

        /// <summary>
        /// Logs an trace description with the specified logging event.
        /// </summary>
        /// <param name="loggingEvent">The logging event</param>
        public static void Trace(LoggingEvent loggingEvent)
        {
            Log.LogWrite(LoggingLevels.Trace, loggingEvent);
        }

        /// <summary>
        /// Logs an trace description with the specified string.
        /// </summary>
        /// <param name="description">The description</param>
        public static void Trace(string description)
        {
            Log.Trace(null, description);
        }

        /// <summary>
        /// Logs a trace description with the specified string.
        /// </summary>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Trace(string format, params object[] args)
        {
            Log.Trace(null, format, args);
        }

        /// <summary>
        /// Logs a trace description with the specified exception and string.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="description">The description</param>
        public static void Trace(Exception exception, string description)
        {
            Log.Trace(new LoggingEvent(description, exception));
        }

        /// <summary>
        /// Logs a trace description with the specified exception and string.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="format">A String containing zero or more format items.</param>
        /// <param name="args">An Object array containing zero or more objects to format.</param>
        public static void Trace(Exception exception, string format, params object[] args)
        {
            Log.Trace(exception, String.Format(CultureInfo.InvariantCulture, format, args));
        }

        #endregion
    }
}
