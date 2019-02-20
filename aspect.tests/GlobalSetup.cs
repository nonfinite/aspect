using Aspect.Utility;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Serilog;

using Splat;

using ILogger = Splat.ILogger;

namespace Aspect.Tests
{
    [TestClass]
    public static class GlobalSetup
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Locator.CurrentMutable.RegisterConstant(new SplatLoggerProxy(), typeof(ILogger));
        }
    }
}
