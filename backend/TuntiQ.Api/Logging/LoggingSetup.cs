using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using Serilog.Ui.Core.Extensions;
using Serilog.Ui.PostgreSqlProvider.Extensions;
using Serilog.Ui.Web.Extensions;

namespace TuntiQ.Api.Logging;

public static class LoggingSetup
{
    private const string TableName = "logs";

    public static IServiceCollection AddAppLogging(this IServiceCollection services, string connectionString)
    {
        var columns = new Dictionary<string, ColumnWriterBase>
        {
            ["Message"] = new RenderedMessageColumnWriter(),
            ["MessageTemplate"] = new MessageTemplateColumnWriter(),
            ["Level"] = new LevelColumnWriter(true, NpgsqlDbType.Varchar),
            ["Timestamp"] = new UtcTimestampColumnWriter(),
            ["Exception"] = new ExceptionColumnWriter(),
            ["LogEvent"] = new LogEventSerializedColumnWriter()
        };

        services.AddSerilog(config => config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.PostgreSQL(connectionString, TableName, columns, needAutoCreateTable: true));

        services.AddSerilogUi(options => options
            .UseNpgSql(opt => opt
                .WithConnectionString(connectionString)
                .WithTable(TableName)));

        return services;
    }

    public static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }

    public static WebApplication UseLogViewer(this WebApplication app, string requiredRole)
    {
        // TODO: Enable when authentication is in place
        //app.UseWhen(
        //    context => context.Request.Path.StartsWithSegments("/serilog-ui"),
        //    branch => branch.Use(async (context, next) =>
        //    {
        //        if (context.User.Identity?.IsAuthenticated != true)
        //        {
        //            await context.ChallengeAsync();
        //            return;
        //        }
        //
        //        if (!context.User.IsInRole(requiredRole))
        //        {
        //            await context.ForbidAsync();
        //            return;
        //        }
        //
        //        await next();
        //    }));

        app.UseSerilogUi();
        return app;
    }
}

public class UtcTimestampColumnWriter() : ColumnWriterBase(NpgsqlDbType.TimestampTz)
{
    public override object GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null) =>
        logEvent.Timestamp.UtcDateTime;
}