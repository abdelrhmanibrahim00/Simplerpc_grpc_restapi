namespace Clients;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Serialization.Hyperion;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Client;
using NLog;
using Services;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Teacher client for classroom simulation.
/// </summary>
class TeacherClient
{
    /// <summary>
    /// A set of names to choose from for the teacher.
    /// </summary>
    /// 
    private readonly List<string> NAMES = new List<string>
    {
        "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace",
        "Hank", "Ivy", "Jack", "Karen", "Leo", "Mia", "Nina"
    };

    // Random number generator
    private static Random random = new Random();
    // Static field for the Teacher object
    private static Teacher teacher = new Teacher
    {
        TeacherId = 0,  // Assign an initial ID
        HasVotedToStart = false,
        HasVotedToEnd = false,
        Name = GetRandomName()  // Random name from NAMES list
    };

    // Method to get a random name from the list
    private static string GetRandomName()
    {
        List<string> names = new List<string>
        {
            "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace",
            "Hank", "Ivy", "Jack", "Karen", "Leo", "Mia", "Nina"
        };
        int randomIndex = random.Next(names.Count);
        return names[randomIndex];
    }

    /// <summary>
    /// Logger for this class.
    /// </summary>
    Logger mLog = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Configures logging subsystem.
    /// </summary>
    private void ConfigureLogging()
    {
        var config = new NLog.Config.LoggingConfiguration();
        var console =
            new NLog.Targets.ConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
            };
        config.AddTarget(console);
        config.AddRuleForAllLevels(console);
        LogManager.Configuration = config;
    }

    /// <summary>
    /// Start assigning values to teacher votes to start the class
    /// </summary>
    private void StartTeacherVoting(Teacher teacher)
    {
        mLog.Info("Teachers are now voting every 2 seconds ");
        Thread.Sleep(2000); // Voting every 2 seconds
        var rnd = new Random();
        Random random = new Random();
        bool randomBool = random.NextDouble() >= 0.6; // 50% chance for true or false
        teacher.HasVotedToStart = randomBool;
        mLog.Info("Teacher has just vote");
    }

    /// <summary>
    /// Start assigning values to teacher votes to End the class
    /// </summary>

    private void EndTeacherVoting(Teacher teacher)
    {
        mLog.Info("Teachers are now voting every 2 seconds ");
        Thread.Sleep(2000); // Voting every 2 seconds
        var rnd = new Random();
        Random random = new Random();
        bool randomBool = random.NextDouble() >= 0.6; // 50% chance for true or false                                      
        teacher.HasVotedToEnd = randomBool;
        mLog.Info("Teacher has just vote");  
    }

    /// <summary>
    /// Program body that simulate the teacher action
    /// </summary>
    private void Run()
    {
        // configure logging
        ConfigureLogging();
        // run everything in a loop to recover from connection errors
        while (true)
        {
            try
            {
             // connect to the server, get service client proxy
                var sc = new ServiceCollection();
                sc
                    .AddSimpleRpcClient(
                        "ClassroomService", // must be the same as defined in the server
                        new HttpClientTransportOptions
                        {
                            Url = "http://127.0.0.1:5000/simplerpc",
                            Serializer = "HyperionMessageSerializer"
                        }
                    )
                    .AddSimpleRpcHyperionSerializer();
                sc.AddSimpleRpcProxy<IClassroomService>("ClassroomService"); // must match the defined service
                var sp = sc.BuildServiceProvider();
                var classroomService = sp.GetService<IClassroomService>();
                while (true)
                {
                  // select a random teacher name and create a Teacher instance
                    if (teacher.TeacherId == 0)
                    {
                        teacher.TeacherId = classroomService.GetUniqueId();
                        mLog.Info($"The teacher {teacher.Name} has got an id {teacher.TeacherId}.");
                    }
                    // log identity data
                    mLog.Info($"I am teacher {teacher.Name}.");
                    // simulate starting class
                    mLog.Info("Teacher has come to the class...");
                    if (!classroomService.IsClassInSession())
                    {
                        mLog.Info("The teacher is checking if there is enough number of students to start the class...");
                        if (classroomService.enoughstudents())
                        {
                            mLog.Info("Theere is enough number of student to start voting");
                            mLog.Info("Teachers are starting to vote to start a class  ...");
                            StartTeacherVoting(teacher);
                            mLog.Info("The votes have been sent to the server to evaluate it");
                            classroomService.VoteStartClass(teacher);
                        }
                    }

                    if (classroomService.IsClassInSession())
                    {
                        mLog.Info("Teachers are starting to vote to END a class  ...");
                        EndTeacherVoting(teacher);
                        classroomService.VoteEndClass(teacher);
                        mLog.Info("Teachers are voting to end a class ...");
                    }
                    // wait for a while to simulate class duration
                    Thread.Sleep(5000); // simulate class duration
                    // wait for a short duration before the next iteration
                    Thread.Sleep(2000);
                }
            }
            catch (Exception e)
            {
                // log whatever exception to console
                mLog.Warn(e, "Unhandled exception caught. Will restart main loop.");
                // prevent console spamming
                Thread.Sleep(2000);
            }
        }
    }

    /// <summary>
    /// Program entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    static void Main(string[] args)
    {
        var self = new TeacherClient();
        self.Run();
    }
}
