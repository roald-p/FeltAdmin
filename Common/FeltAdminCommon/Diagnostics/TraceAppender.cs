using System;
using System.Diagnostics;
using System.Globalization;

namespace FeltAdmin.Diagnostics
{
    /// <summary>
    /// Provides a simple appender that directs tracing or debugging output to the Win32 OutputDebugString function.
    /// </summary>
    public class TraceAppender : LogAppender
    {
        /// <summary>
        /// Initializes a new instance of the TraceAppender class. Default Logging level is LoggingLeveles.Trace.
        /// </summary>
        public TraceAppender() : this(LoggingLevels.Trace) { }

        /// <summary>
        /// Initializes a new instance of the TraceAppender class with the specified logging level.
        /// </summary>
        public TraceAppender(LoggingLevels level) : base(level, LoggingLevels.Trace) { }

        /// <summary>
        /// Initializes a new instance of the TraceAppender class with the specified logging level.
        /// </summary>
        public TraceAppender(LoggingLevels level, LoggingLevels minLoggingLevel) : base(level, minLoggingLevel)
        {
        }

        #region LogAppender Members

        public override void Append(LoggingLevels level, LoggingEvent loggingEvent)
        {
            if (loggingEvent != null)
            {
                string details = String.IsNullOrEmpty(loggingEvent.ExceptionDescription) ? "" : String.Format(CultureInfo.InvariantCulture, " - Details: {0}", loggingEvent.ExceptionDescription);
                Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}{1}", loggingEvent.UserMessage, details));
            }
        }

        #endregion
    }
}
