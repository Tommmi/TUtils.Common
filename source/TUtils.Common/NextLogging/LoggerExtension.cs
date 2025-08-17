using System;
using System.Reflection.Metadata;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.Log4Net;
using TUtils.Common.Logging.LogMocs;
using TUtils.Common.Tasks;

namespace TUtils.Common.Logging
{
	public static class LoggerExtension
	{
        private static object sync = new object();
        private static Func<ICallerContext> _createCallerContext = null;

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

        public static void InitializeConsoleLogging(this object loggingInstance, LogSeverityEnum minLogSeverityEnum, CallerContextDto callerContextDto = null)
        {
            lock (sync)
            {
                if(_createCallerContext == null)
                {
                    _createCallerContext = () =>
                    {
                        var logWriter = new LogConsoleWriter(
                            minSeverity: minLogSeverityEnum,
                            namespacesWhiteList: ["*"],
                            namespacesBlackList: []);
                        return CreateCallersContent(callerContextDto, logWriter);
                    };
                }
            }
        }

        public static void InitializeLog4NetLogging(this object loggingInstance, CallerContextDto callerContextDto = null)
        {
            lock (sync)
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

        private static ICallerContext CreateCallersContent(CallerContextDto callerContextDto, ILogWriter logWriter)
        {
            ITLog log = new TLog(logWriter: logWriter, isLoggingOfMethodNameActivated: true);
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