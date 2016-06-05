using System;
using System.Globalization;

namespace FeltAdmin.Diagnostics
{
    /// <summary>
    /// Abstract base class for log appenders. All classes that represent appenders inherit from this class.
    /// </summary>
    public abstract class LogAppender : ILogAppender
    {
        private LoggingLevels m_loggingLevel;
        private LoggingLevels m_minLoggingLevel;

        #region ILogAppender Members

        protected LogAppender() : this(LoggingLevels.Trace) { }

        protected LogAppender(LoggingLevels loggingLevel) : this(loggingLevel, LoggingLevels.None) { }

        protected LogAppender(LoggingLevels loggingLevel, LoggingLevels minLoggingLevel)
        {
            this.MinLoggingLevel = minLoggingLevel;
            this.LoggingLevel = loggingLevel;
        }

        /// <summary>
        /// Gets or sets a value that 
        /// </summary>
        public LoggingLevels LoggingLevel
        {
            get
            {
                return this.m_loggingLevel;
            }
            set
            {
                if (value > this.MinLoggingLevel)
                {
                    this.m_loggingLevel = this.MinLoggingLevel;
                }
                else
                {
                    this.m_loggingLevel = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that 
        /// </summary>
        public LoggingLevels MinLoggingLevel
        {
            get
            {
                return this.m_minLoggingLevel;
            }
            set
            {
                if (this.LoggingLevel > value)
                {
                    this.LoggingLevel = value;
                }

                this.m_minLoggingLevel = value;
            }
        }

        //public abstract void WriteLine(LoggingLevels level, string message, Exception exception);

        public abstract void Append(LoggingLevels level, LoggingEvent loggingEvent);

        ///// <summary>
        ///// Returns a formatted message that contains both the message and the exception details.
        ///// </summary>
        ///// <param name="message">The message to format</param>
        ///// <param name="exception">The exception to format</param>
        ///// <returns>The combined message</returns>
        public virtual string FormatMessage(LoggingLevels level, LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }
            if (!String.IsNullOrEmpty(loggingEvent.UserMessage) &&
                !String.IsNullOrEmpty(loggingEvent.ExceptionDescription))
            {
                return String.Format(CultureInfo.InvariantCulture,"{0} - Details: {1}", loggingEvent.UserMessage, loggingEvent.ExceptionDescription);
            }
            else if (!String.IsNullOrEmpty(loggingEvent.UserMessage))
            {
                return loggingEvent.UserMessage;
            }
            else if (!String.IsNullOrEmpty(loggingEvent.ExceptionDescription))
            {
                return loggingEvent.ExceptionDescription;
            }
            else
            {
                return loggingEvent.ToString();
            }
        }

        #endregion
    }
}
