using System;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace FeltAdmin.Diagnostics
{
    /// <summary>
    /// Entity class used for logging to COERRORLOG.
    /// </summary>
    public class LoggingEvent
    {
        public static readonly string FormattedUserMessage = "Message: {0}{1}";
        public static readonly string FormattedExceptionDescription = "Details: {0}{1}";
        private Exception m_sourceException;

        private static string s_assemblyVersionNumber;
        private static readonly object s_lockObject = new object();

        public LoggingEvent() : this(String.Empty, null) { }

        public LoggingEvent(string message) : this(message, null) { }

        public LoggingEvent(Exception exception) : this(String.Empty, exception) { }

        public LoggingEvent(string message, Exception exception)
        {
            if (exception == null)
            {
                SetDefaultValues(this);
                this.UserMessage = message;
            }
            else
            {
                this.UserMessage = String.IsNullOrEmpty(message) ? exception.Message : message;
                this.ExceptionDescription = GetExceptionDescription(exception);
                this.SourceException = exception;

               
                    SetDefaultValues(this);
                    this.ExceptionClass = exception.GetType().Name;
              
            }
        }

        private static string GetExceptionDescription(Exception exception)
        {
            var sb = new StringBuilder();
           
           
            sb.AppendLine(exception.ToString());
            return sb.ToString();
        }

         /// <summary>
        /// The terminal id the client is exceuting on.
        /// </summary>
      // <summary>
        /// The user name (the name which the user logged into DIPS).
        /// </summary>
         /// <summary>
        /// The active patient in the DIPS client when the error occured, will only be logged in the client and not on the server.
        /// </summary>
       
        /// <summary>
        /// DIPS versionnumber.
        /// </summary>
        public string VersionNumber { get; set; }

        /// <summary>
        /// Identificator for the error incident this LoggingEvent is assosiated with.
        /// </summary>
       
        /// <summary>
        /// The date and time when the error occured.
        /// </summary>
        public DateTime ExceptionTime { get; set; }

        /// <summary>
        /// Exception class type.
        /// </summary>
        public string ExceptionClass { get; set; }

        /// <summary>
        /// User friendly exception message.
        /// </summary>
        public string UserMessage { get; set; }

        /// <summary>
        /// Full exception description, that is, the full exception and stack trace.
        /// </summary>
        public string ExceptionDescription { get; set; }

        /// <summary>
        /// SQL statement running when the exception occured (if any).
        /// </summary>
        public string SqlStatement { get; set; }

        /// <summary>
        /// SOAP request sent from client to WS (if any).
        /// </summary>
        public string SoapMessage { get; set; }

        /// <summary>
        /// The name of the module where the error was raised.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The source exception (if any).
        /// </summary>
        [XmlIgnore]
        public Exception SourceException
        {
            get { return this.m_sourceException; }
            set { this.m_sourceException = value; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(FormattedUserMessage, this.UserMessage, Environment.NewLine);
            sb.AppendFormat(FormattedExceptionDescription, this.ExceptionDescription, Environment.NewLine);

            return sb.ToString();
        }

        #region static helper functions

        private static void SetDefaultValues(LoggingEvent loggingEvent)
        {
            loggingEvent.ExceptionTime = DateTime.Now;
            loggingEvent.Source = System.Threading.Thread.GetDomain().FriendlyName;

            

            loggingEvent.VersionNumber = GetAssemblyVersionNumber();
        }

        //// TODO: Move to appropriate class
        private static string GetAssemblyVersionNumber()
        {
            if (String.IsNullOrEmpty(s_assemblyVersionNumber))
            {
                lock (s_lockObject)
                {
                    if (String.IsNullOrEmpty(s_assemblyVersionNumber))
                    {
                        Assembly entryAssembly = Assembly.GetEntryAssembly();
                        if (entryAssembly != null)
                        {
                            var customAttributes =
                                entryAssembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                            AssemblyFileVersionAttribute fileVersionAttribute = null;

                            if (customAttributes.Length > 0)
                            {
                                fileVersionAttribute =
                                    customAttributes[0] as AssemblyFileVersionAttribute;
                            }

                            s_assemblyVersionNumber = (fileVersionAttribute != null) ? fileVersionAttribute.Version : "<unknown>";
                        }
                        else
                        {
                            s_assemblyVersionNumber = "<unknown>";
                        }
                    }
                }
            }

            return s_assemblyVersionNumber;
        }
        #endregion
    }
}
