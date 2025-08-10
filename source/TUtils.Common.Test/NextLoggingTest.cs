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
    /// <summary>
    /// Test suite for the NextLogging framework functionality.
    /// Tests the modern logging API with task-based context propagation and caller context handling.
    /// </summary>
    [TestClass]
    public class NextLoggingTest
    {
        /// <summary>
        /// Tests the NextLogging framework with context propagation across tasks.
        /// Verifies that logging values can be set and retrieved across different execution contexts.
        /// </summary>
        /// <returns>A task representing the asynchronous test operation.</returns>
        [TestMethod]
        public async Task TestNextLogging()
        {
            // Arrange: Initialize console logging with INFO level and set context value
            this.InitializeConsoleLogging(minLogSeverityEnum: LogSeverityEnum.INFO);
            this.Log().SetLoggingValue(valueName: "a", value: "123");
            
            // Act: Execute logging operation in a separate task
            await Task.Run(RunSubTask);
        }

        /// <summary>
        /// Helper method that runs in a separate task to test context propagation.
        /// Verifies that the caller context and logging values are properly preserved across task boundaries.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RunSubTask()
        {
            // Simulate some async work
            await Task.Delay(1000);
            
            // Act: Perform logging with structured data
            this.Log().LogInfo(map: () => new { descr = "hello world" });
            
            // Assert: Verify that the caller context is preserved and contains expected values
            var callContext = new TaskStorage<ICallerContext>(id: "TUtils-Logger").Value;
            callContext.Should().NotBeNull("Caller context should be available in the task");
            callContext["a"].Should().Be("123", "Context value 'a' should be preserved across task boundaries");
        }
    }
}
