using Serilog;
using System.Reflection;
using Sharpcord_bot_library;

internal class Program
{
    static List<Tuple<Assembly,Type>> LoadedBots = new List<Tuple<Assembly,Type>>();
    static List<Bot> BotInstances = new List<Bot>();
    static void Main(string[] args)
    {
        LoggerConfiguration cfg =
            new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + "/logs/log-" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt");
        if (args.Contains("-D"))
        {
            cfg.MinimumLevel.Debug();
        }
        else
        {
            cfg.MinimumLevel.Information();
        }
        Log.Logger = cfg.CreateLogger();
        Log.Information($"ENVIRONMENT: {Environment.OSVersion.VersionString}, {Environment.ProcessorCount} core CPU, {Environment.MachineName}; Running as {Environment.UserName}.");
        Log.Debug("Debug messages enabled.");
        Console.CancelKeyPress +=Console_CancelKeyPress;
        if (Environment.UserName == "root")
        {
            Log.Warning("Running as root! This poses several security risks. Any vulnerability in any module can be fatal to the entire host machine. To avoid these risks, run Sharpcord as a regular user with specific permissions.");
        }
        Log.Information($"Running Sharpcord version 1.0 by Mukunya");
        WebhookServer.LogDebug += (object? sender, string s) => { Log.Debug("Webhook server  |  " + s); };
        WebhookServer.LogInfo += (object? sender, string s) => { Log.Information("Webhook server  |  " + s); };
        WebhookServer.LogWarn += (object? sender, string s) => { Log.Warning("Webhook server  |  " + s); };
        WebhookServer.LogError += (object? sender, Exception s) => { Log.Error(s, "Webhook server  |  "); };
        Log.Information("Starting, Searching for bots");
        string[] botdlls = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/modules");
        foreach (string botdll in botdlls)
        {
            if (botdll.EndsWith(".dll"))
            {
                Assembly dll = Assembly.LoadFile(botdll);
                foreach (var type in dll.GetExportedTypes())
                {
                    if (type.IsDefined(typeof(DiscordBotAttribute),false))
                    {
                        LoadedBots.Add(new(dll,type));
                        Log.Information($"Loaded {type.GetCustomAttribute<DiscordBotAttribute>()?.DisplayName}, a bot with type {type.GetCustomAttribute<DiscordBotAttribute>()?.Type}");
                    }
                }
            }
        }
        Log.Information($"Loading process done, loaded {LoadedBots.Count} modules.");
        Log.Information("Starting bots...");
        foreach (var item in LoadedBots)
        {
            try
            {
                Log.Information("Starting " + item.Item2.GetCustomAttribute<DiscordBotAttribute>()?.DisplayName + "...");
                BotInstances.Add(item.Item1.CreateInstance(item.Item2.FullName) as Bot);
                BotInstances.Last().LogInfo += (object? sender, string s) => { Log.Information(item.Item2.GetCustomAttribute<DiscordBotAttribute>()?.DisplayName + "  |  " + s); };
                BotInstances.Last().LogWarn += (object? sender, string s) => { Log.Warning(item.Item2.GetCustomAttribute<DiscordBotAttribute>()?.DisplayName + "  |  " + s); };
                BotInstances.Last().LogError += (object? sender, Exception s) => { Log.Error(s,item.Item2.GetCustomAttribute<DiscordBotAttribute>()?.DisplayName + "  |  "); };
                Log.Information("Done.");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed. ");
            }
        }
        new ManualResetEvent(false).WaitOne();
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Log.Information("Shutting down, ctrl+c pressed.");
        Log.CloseAndFlush();
        Environment.Exit(1);
    }
}