using System;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.Log4Net;
using TUtils.Common.Logging.LogMocs;
using TUtils.Common.Tasks;

#pragma warning disable IDE0130
namespace TUtils.Common.Logging
#pragma warning restore IDE0130
{
	public static class LoggerExtension
	{
        private static object _sync = new ();
        private static Func<ICallerContext> _createCallerContext;

        public static ILogger Log(this object loggingInstance, ICallerContext callContext)
        {
            return callContext.LoggerFactory.Create(loggingInstance, callContext);
        }
        public static ILogger Log(this object loggingInstance)
        {
            var storage = GetTaskStorage();

            return storage.Value.LoggerFactory.Create(loggingInstance, storage.Value);
        }

        private static TaskStorage<ICallerContext> GetTaskStorage()
        {
            var storage = new TaskStorage<ICallerContext>(id:"TUtils-Logger");

            if(storage.Value == null)
            {
                if(_createCallerContext == null)
                {
                    throw new NullReferenceException("callContext");
                }
                storage.Value = _createCallerContext();
            }

            return storage;
        }

        public static void InitializeConsoleLogging(
	        this object loggingInstance, 
	        LogSeverityEnum minLogSeverityEnum, 
	        CallerContextDto callerContextDto = null,
	        bool writeToDebug = false)
        {
            lock (_sync)
            {
                if(_createCallerContext == null)
                {
                    _createCallerContext = () =>
                    {
                        var logWriter = new LogConsoleWriter(
                            minSeverity: minLogSeverityEnum,
                            namespacesWhiteList: ["*"],
                            namespacesBlackList: [],
                            writeToDebug: writeToDebug);
                        return CreateCallersContent(callerContextDto, logWriter);
                    };
                }
            }
        }

        public static void InitializeLog4NetLogging(this object loggingInstance, CallerContextDto callerContextDto = null)
        {
            lock (_sync)
            {
                if (_createCallerContext == null)
                {
                    _createCallerContext = () =>
                    {
                        ILogWriter logWriter = new Log4NetWriter();
                        return CreateCallersContent(callerContextDto, logWriter);
                    };
                }
            }
        }

        public static void InitializeLog4IndividualLogging(
	        this object loggingInstance, 
            ILogWriter individualLogWriter,
	        CallerContextDto callerContextDto = null)
        {
	        lock (_sync)
	        {
		        if (_createCallerContext == null)
		        {
			        _createCallerContext = () =>
			        {
				        return CreateCallersContent(callerContextDto, individualLogWriter);
			        };
		        }
	        }
        }

        private static ICallerContext CreateCallersContent(CallerContextDto callerContextDto, ILogWriter logWriter)
        {
            ITLog log = new TLog(logWriter: logWriter, isLoggingOfMethodNameActivated: false);
            ILoggerFactory loggerFactory = new LoggerFactory(log: log);
            ICallerContext callerContext = new CallerContext(loggerFactory: loggerFactory, callerContextDto: callerContextDto);
            return callerContext;
        }

        public static void SetLoggingValue(this ILogger loggingInstance, string valueName, string value)
        {
            GetTaskStorage().Value[valueName] = value;
        }
    }
}