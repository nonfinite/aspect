using System;
using System.Runtime.CompilerServices;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Aspect.Utility
{
    public static class LogEx
    {
        public static ILogger Log<T>(this T _, [CallerMemberName] string memberName = null) =>
            Serilog.Log.ForContext(new ClassEnricher(typeof(T), memberName));

        private sealed class ClassEnricher : ILogEventEnricher
        {
            public ClassEnricher(Type type, string memberName)
            {
                mType = type;
                mMemberName = memberName;
            }

            private readonly string mMemberName;
            private readonly Type mType;

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SourceType", mType.FullName));
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SourceMember", mMemberName));
                logEvent.AddPropertyIfAbsent(propertyFactory
                    .CreateProperty(Constants.SourceContextPropertyName, $"{mType.FullName}::{mMemberName}"));
            }
        }
    }
}
