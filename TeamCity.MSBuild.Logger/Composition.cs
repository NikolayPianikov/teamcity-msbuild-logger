// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Local
namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;
    using EventHandlers;
    using JetBrains.TeamCity.ServiceMessages.Read;
    using JetBrains.TeamCity.ServiceMessages.Write;
    using JetBrains.TeamCity.ServiceMessages.Write.Special;
    using JetBrains.TeamCity.ServiceMessages.Write.Special.Impl.Updater;
    using Pure.DI;
    using static Pure.DI.Lifetime;

    internal partial class Composition
    {
        private static void Setup() =>
            DI.Setup()
                .Hint(Hint.Resolve, "Off")
                .Root<INodeLogger>("Logger")

                .DefaultLifetime(Singleton)
                    .Bind().To<NodeLogger>()
                    .Bind().To<Environment>()
                    .Bind().To<Diagnostics>()
                    .Bind().To<LoggerContext>()
                    .Bind().To<DefaultConsole>()
                    .Bind().To<Parameters>()
                    .Bind().To<BuildEventManager>()
                    .Bind().To<DeferredMessageWriter>()
                    .Bind().To<MessageWriter>()
                    .Bind().Bind<IEventRegistry>().To<EventContext>()
                    .Bind().To<HierarchicalMessageWriter>()
                    .Bind().Tags(ColorMode.TeamCity, TeamCityMode.SupportHierarchy).To<TeamCityHierarchicalMessageWriter>()
                    .Bind().To<LogWriter>()
                    .Bind().To<ColorTheme>()
                
                    // Statistics
                    .Bind().To<Statistics>()
                    .Bind(StatisticsMode.Default).To<DefaultStatistics>()
                    .Bind(StatisticsMode.TeamCity).To<TeamCityStatistics>()
                
                    // Build event handlers
                    .Bind().To<BuildFinishedHandler>()
                    .Bind().To<BuildStartedHandler>()
                    .Bind().To<CustomEventHandler>()
                    .Bind().To<ErrorHandler>()
                    .Bind().To<MessageHandler>()
                    .Bind().To<ProjectFinishedHandler>()
                    .Bind().To<ProjectStartedHandler>()
                    .Bind().To<TargetFinishedHandler>()
                    .Bind().To<TargetStartedHandler>()
                    .Bind().To<TaskFinishedHandler>()
                    .Bind().To<TaskStartedHandler>()
                    .Bind().To<WarningHandler>()

                    .Bind().To<TeamCityServiceMessages>()
                    .Bind().To<FlowIdGenerator>()
                    .Bind<DateTime>().As(Transient).To(_ => DateTime.Now)
                    .Bind(Tag.Type).To<BuildErrorMessageUpdater>()
                    .Bind(Tag.Type).To<BuildWarningMessageUpdater>()
                    .Bind(Tag.Type).To<BuildMessageMessageUpdater>()
                    .Bind<ITeamCityWriter>().To(
                        ctx =>
                        {
                            ctx.Inject(out ITeamCityServiceMessages messages);
                            ctx.Inject(ColorMode.NoColor, out ILogWriter logWriter);
                            return messages.CreateWriter(str => logWriter.Write(str + "\n"));
                        })
                
                .DefaultLifetime(Transient)
                    .Bind().To<PerformanceCounter>()
                    .Bind().To<ColorStorage>()
                    
                .DefaultLifetime(PerBlock)
                    .Bind().To<StringService>()
                    .Bind().To<PathService>()
                    .Bind().To<ParametersParser>()
                    .Bind().To<PerformanceCounterFactory>()
                    .Bind().To<LogFormatter>()
                    .Bind().To<EventFormatter>()
                    .Bind(ColorMode.NoColor).To<NoColorLogWriter>()
                    .Bind(ColorMode.AnsiColor).To<AnsiLogWriter>()
                    .Bind(ColorMode.Default).To<DefaultLogWriter>()
                    .Bind(TeamCityMode.Off).To<DefaultHierarchicalMessageWriter>()
                    .Bind().To<ServiceMessageFormatter>()
                    .Bind(Tag.Type).To<TimestampUpdater>()
                    .Bind().To<ServiceMessageParser>()
                    .Bind(ColorThemeMode.Default).To<DefaultColorTheme>()
                    .Bind(ColorThemeMode.TeamCity).To<TeamCityColorTheme>();
    }
}
