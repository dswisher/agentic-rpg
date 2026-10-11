// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using Serilog.Core;
using Serilog.Events;

namespace AgenticRpg.Cli
{
    public sealed class ShortSourceContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.TryGetValue("SourceContext", out var value) &&
                value is ScalarValue { Value: string sourceContext })
            {
                var lastDot = sourceContext.LastIndexOf('.');
                var name = sourceContext[(lastDot + 1)..];

                logEvent.AddPropertyIfAbsent(
                    propertyFactory.CreateProperty("SourceContextShort", name));
            }
        }
    }
}
