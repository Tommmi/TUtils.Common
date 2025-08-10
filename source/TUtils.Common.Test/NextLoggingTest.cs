using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using TUtils.Common.Logging;
using TUtils.Common.Logging.Common;
using TUtils.Common.Logging.LogMocs;
using TUtils.Common.Tasks;
using ILoggerFactory = TUtils.Common.Logging.ILoggerFactory;
using LoggerFactory = TUtils.Common.Logging.LoggerFactory;

namespace TUtils.Common.Test
{
    [TestClass]
    public class NextLoggingTest
    {
        [TestMethod]
        public async Task TestNextLogging()
        {
            this.InitializeConsoleLogging(minLogSeverityEnum:LogSeverityEnum.INFO);
            this.SetLoggingValue(valueName:"a",value:"123");
            await Task.Run(RunSubTask);
        }

        private async Task RunSubTask()
        {
            await Task.Delay(1000);
            this.Log().LogInfo(map: () => new { descr="hello world"});
            var callContext = new TaskStorage<ICallerContext>(id: "TUtils-Logger").Value;
            callContext.Should().NotBeNull();
            callContext["a"].Should().Be("123");
        }
    }
}
