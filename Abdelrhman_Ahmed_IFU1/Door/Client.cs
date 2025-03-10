namespace Clients;

using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
using NLog;
using Services;

class DoorClient
{
    // A set of names and surnames to choose from.
    private readonly List<string> NAMES = new List<string> { "John", "Peter", "Jack", "Steve" };
    private readonly List<string> SURNAMES = new List<string> { "Johnson", "Peterson", "Jackson", "Steveson" };
    
    // Logger for this class.
    private Logger mLog = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Logging configuration.
    /// </summary>
    private void ConfigureLogging()
    {
        var config = new NLog.Config.LoggingConfiguration();
        var console = new NLog.Targets.ConsoleTarget("console")
        {
            Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
        };
        config.AddTarget(console);
        config.AddRuleForAllLevels(console);
        LogManager.Configuration = config;
    }

    /// <summary>
    /// Program entry point.
    /// </summary>
    private void Run()
    {
        // Configure logging
        ConfigureLogging();

        var rnd = new Random();

        // Run everything in a loop to recover from connection errors
        while (true)
        {
            try
            {
                // Connect to the server and get service client proxy
                var sc = new ServiceCollection();
                sc.AddSimpleRpcClient(
                    "ClassroomService",
                    new HttpClientTransportOptions
                    {
                        Url = "http://127.0.0.1:5000/simplerpc",
                        Serializer = "HyperionMessageSerializer"
                    })
                    .AddSimpleRpcHyperionSerializer();
                sc.AddSimpleRpcProxy<IClassroomService>("ClassroomService"); 
                var sp = sc.BuildServiceProvider();
                var classroomService = sp.GetService<IClassroomService>();
                int uniqueId = 0; // Assuming this method returns a unique ID for the door
                var door = new DoorDesc(uniqueId, 0, false, true, "Main entrance door");
                // Log identity data
                mLog.Info($"Door {door.DoorId} initialized. Description: {door.Description}.");

                // Door interaction logic
                while (true)
                {
                    // Simulate door behavior
                    
                    if (!classroomService.IsClassInSession())
                    {
                        // Simulate students arriving
                        int arrivingStudents = rnd.Next(-3, 10); // Random number of students (-5 to 5)
                        door.AmountOfStudents = arrivingStudents;
                        mLog.Info($"{arrivingStudents} students have been generated at Door {door.DoorId}. Total: {door.AmountOfStudents}");
                        classroomService.Generatednumberofstudnent(door);
                        mLog.Info($"{arrivingStudents} students have been sent to the server. Total students at Door {door.DoorId}: {door.AmountOfStudents}");
                        // Notify classroom service about the students
                        Thread.Sleep(rnd.Next(1000, 3000));
                        // Wait for a bit before the next batch of students
                    }
                    else
                    {
                        mLog.Info($"Door {door.DoorId} is closed. No students can enter.");
                        Thread.Sleep(2000); // Wait before checking again
                    }
                }
            }
            catch (Exception e)
            {
                // Log any exception to console
                mLog.Warn(e, "Unhandled exception caught. Will restart main loop.");
                Thread.Sleep(2000);
            }
        }
    }

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param
    static void Main(string[] args)
    {
        var self = new DoorClient();
        self.Run();
    }
}
