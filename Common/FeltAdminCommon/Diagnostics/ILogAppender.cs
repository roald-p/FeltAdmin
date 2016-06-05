namespace FeltAdmin.Diagnostics
{
    /// <summary>
    /// Interface implemented by log appenders
    /// </summary>
    public interface ILogAppender
    {
        LoggingLevels LoggingLevel
        {
            get;
            set;
        }

        LoggingLevels MinLoggingLevel
        {
            get;
            set;
        }

        //void WriteLine(LoggingLevels level, string message, Exception exception);
        void Append(LoggingLevels level, LoggingEvent loggingEvent);
        string FormatMessage(LoggingLevels level, LoggingEvent loggingEvent);
    }
}
