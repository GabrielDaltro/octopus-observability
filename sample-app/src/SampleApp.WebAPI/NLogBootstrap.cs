using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace SampleApp.WebAPI
{
    public static class NLogBootstrap
    {
        public static void Configure(IConfiguration configuration, IHostEnvironment environment)
        {
            GlobalDiagnosticsContext.Set("service.name",environment.ApplicationName);
            GlobalDiagnosticsContext.Set("service.environment", environment.EnvironmentName);
            GlobalDiagnosticsContext.Set("service.version", typeof(Program).Assembly.GetName().Version?.ToString());

            LogManager.Configuration = BuildConfiguration(configuration);
        }

        private static LoggingConfiguration BuildConfiguration(IConfiguration configuration)
        {
            var config = new LoggingConfiguration();

            var blackhole = new NullTarget("blackhole");

            var fileTarget = new FileTarget("elastic-file")
            {
                FileName = configuration["Observability:FileName"] ?? "${basedir}/logs/service.ndjson",
                KeepFileOpen = true,
                Layout = BuildJsonLayout()
            };

            var consoleTarget = new ConsoleTarget("console")
            {
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}"
            };

            config.AddTarget(blackhole);
            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget, "*");

            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, blackhole, "Microsoft.Extensions.*", final: true);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, blackhole, "Microsoft.Hosting.*", final: true);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, blackhole, "Microsoft.AspNetCore.*", final: true);

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget, "*");

            return config;
        }


        private static JsonLayout BuildJsonLayout()
        {
            var layout = new JsonLayout
            {
                SuppressSpaces = true,
                ExcludeEmptyProperties = true,
                IncludeEventProperties = false,
                IncludeScopeProperties = false
            };

            layout.Attributes.Add(new JsonAttribute("@timestamp", "${date:universalTime=true:format=o}"));
            layout.Attributes.Add(new JsonAttribute("message", "${message:raw=true}"));

            layout.Attributes.Add(new JsonAttribute("trace", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("id", "${scopeproperty:item=trace.id}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("span", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("id", "${scopeproperty:item=span.id}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("transaction", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("id", "${scopeproperty:item=transaction.id}")
                }
            }, false));


            layout.Attributes.Add(new JsonAttribute("service", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("name", "${gdc:item=service.name}"),
                    new JsonAttribute("version", "${gdc:item=service.version}"),
                    new JsonAttribute("environment", "${gdc:item=service.environment}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("log", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("level", "${level}"),
                    new JsonAttribute("logger", "${logger}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("product", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("id", "${scopeproperty:item=product.id}"),
                    new JsonAttribute("name", "${scopeproperty:item=product.name}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("user", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("id", "${scopeproperty:item=user.id}"),
                    new JsonAttribute("name", "${scopeproperty:item=user.name}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("error", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("type", "${exception:format=type}"),
                    new JsonAttribute("message", "${exception:format=message}"),
                    new JsonAttribute("stack_trace", "${exception:format=tostring}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("event", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("action", "${scopeproperty:item=event.action}"),
                    new JsonAttribute("outcome", "${scopeproperty:item=event.outcome}")
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("http", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("request", new JsonLayout
                    {
                        Attributes =
                        {
                            new JsonAttribute("method", "${scopeproperty:item=http.request.method}")
                        }
                    }, false),
                    new JsonAttribute("response", new JsonLayout
                    {
                        Attributes =
                        {
                            new JsonAttribute("status_code", "${scopeproperty:item=http.response.status_code}")
                        }
                    }, false)
                }
            }, false));

            layout.Attributes.Add(new JsonAttribute("url", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("full", "${scopeproperty:item=url.full}"),
                    new JsonAttribute("path", "${scopeproperty:item=url.path}"),
                    new JsonAttribute("query", "${scopeproperty:item=url.query}")
                }
            }, false));

            return layout;
        }

    }
}
