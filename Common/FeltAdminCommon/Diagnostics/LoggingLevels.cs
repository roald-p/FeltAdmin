using System;

namespace FeltAdmin.Diagnostics
{
#if !PocketPC    
    [Serializable]
#endif
    public enum LoggingLevels
    {
        None = 0,
        /// <summary>
        /// Output error-handling messages.
        /// </summary>
        Error = 10,
         
        /// <summary>
        /// Output informational messages, warnings, messages.
        /// </summary>
        Warning = 15,
        
        /// <summary>
        /// Output informational messages, warnings, and error-handling messages.
        /// </summary>
        Info = 20,
        /// <summary>
        /// Output tracing and informational messages, warnings, and error-handling messages.
        /// </summary>
        Trace = 30,
        
       
    }
}
