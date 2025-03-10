namespace Servers;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using NLog;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/// <summary>
/// Classroom state descriptor.
/// </summary>
public class ClassroomState
{
    /// <summary>
    /// Access lock.
    /// </summary>
    public readonly object AccessLock = new object();

    /// <summary>
    /// Class in session state.
    /// </summary>
    public bool ClassInSession = false;
    
    /// <summary>
    /// Current number of students in the class.
    /// </summary>
    public int StudentCount = 0;

    /// <summary>
    /// Threshold to start the class .
    /// </summary>
    public readonly int StartThreshold = 3;

    /// <summary>
    /// Check whether or not  the votes were enough to start or end the class session
    /// </summary>
    public bool AreVotesStartSufficient(Dictionary<int, bool> votesStart)
    {
        // Check if there is more than one element in the dictionary
        if (votesStart.Count <= 1)
        {
            return false;
        }
        // Count the number of true values in the dictionary
        int trueCount = votesStart.Values.Count(v => v);
        // Calculate the ratio of true values to the total number of votes
        double ratio = (double)trueCount / votesStart.Count;
        // Define the threshold (for example, 0.5 for 50%)
        double threshold = 0.5;
        // Return true if the ratio is greater than the threshold, otherwise false
        return ratio > threshold;
    }

}

/// <summary>
/// <para>Classroom logic.</para>
/// <para>Thread safe.</para>
/// </summary>
class ClassroomLogic
{
    public int LastUniqueId;   // int that counts how many teachers
    private Logger mLog = LogManager.GetCurrentClassLogger();  // using logManager

    /// <summary>
    /// Accumulated teacher VotesStart for starting the class that has every teacher Id and his vote
    /// </summary>
    private Dictionary<int, bool> VotesStart = new Dictionary<int, bool>();

    /// <summary>
    /// Accumulated teacher VotesEnd for ending the class that has every teacher Id and his vote
    /// </summary>
    private Dictionary<int, bool> VotesEnd = new Dictionary<int, bool>();

    // Object of calssroom state that will hold the class state and used to evaluate the votes
    private ClassroomState mState = new ClassroomState();

    private int generatedstudent { get; set; } // accumulate the students numbers that are being sent from door

    /// <summary>
    /// increment the number of generated students from door client class.
    /// </summary>
    public void studnetnsnumber(int a)
    {
        lock (mState.AccessLock)
        {
            generatedstudent += a;
        }
    }

    /// <summary>
    /// Determines if there are enough students to start the voting process.
    /// </summary>
    public bool canvotingstart()
    {
        if (generatedstudent >= mState.StartThreshold && !mState.ClassInSession)
        {
            mLog.Info("there is enough student to start the voting");
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Get next unique ID from the server. This can be used for assigning IDs to doors or teachers.
    /// </summary>
    /// <returns>Unique ID.</returns>
    public int GetUniqueId()
    {
        lock (mState.AccessLock)
        {
            LastUniqueId += 1;
            return LastUniqueId;
        }
    }

    /// <summary>
    /// Get current class session state.
    /// </summary>
    /// <returns>True if class is in session, false otherwise.</returns>				
    public bool IsClassInSession()
    {
        lock (mState.AccessLock)
        {
            return mState.ClassInSession;
        }
    }

    /// <summary>
    /// Teachers vote to start the class.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher voting.</param>
    /// <returns>True if vote is registered, false if already voted or class is in session.</returns>
    public bool VoteToStartClass(Teacher teacher)
    {
        lock (mState.AccessLock)
        {
            if (VotesStart.ContainsKey(teacher.TeacherId))
            {
                VotesStart[teacher.TeacherId] = teacher.HasVotedToStart;   // Update value if the key exists
            }
            else
            {
                VotesStart.Add(teacher.TeacherId, teacher.HasVotedToStart);  // Add a new key-value pair if the key does not exist
            }
            // if the class is already in session
            if (mState.ClassInSession)  
            {
                mLog.Info($"Teacher {teacher.TeacherId} has already voted or class is in session.");
                return false;
            }

            // Inform that the server got a teacher vote            
            mLog.Info($"Teacher {teacher.TeacherId} voted to start the class.");
            // count the length of vote start dictionary
            int length = VotesStart.Count;
            // check if all teachers votes are submitted
            if (length == LastUniqueId)
            {
                if (mState.AreVotesStartSufficient(VotesStart))
                {
                    StartClass();
                    mLog.Info("VotesStart were sufficient to start the class");
                    return true;
                }
            }
            // If the number of VotesStart exceeds half, start the class
            else mLog.Info("VotesStart were not sufficient to start the class");
            return false;
        }
    }

    /// <summary>
    /// Teachers vote to end the class.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher voting.</param>
    /// <returns>True if vote is registered, false if already voted or class is not in session.</returns>
    public bool VoteToEndClass(Teacher teacher)
    {
        lock (mState.AccessLock)
        {
            if (VotesEnd.ContainsKey(teacher.TeacherId))
            {
                // Update the existing value if the key exists
                VotesEnd[teacher.TeacherId] = teacher.HasVotedToEnd;
            }
            else
            {
                // Add a new key-value pair if the key does not exist
                VotesEnd.Add(teacher.TeacherId, teacher.HasVotedToEnd);
            }
            // Inform that the server got a teacher vote         
            mLog.Info($"Teacher {teacher.TeacherId} voted to  End class.");
            // count the length of vote start dictionary 
            int length = VotesEnd.Count;
            // check if all teachers votes are submitted
            if (length == LastUniqueId)
            {
                if (mState.AreVotesStartSufficient(VotesEnd))
                {
                   EndClass();
                    mLog.Info("VotesEnd were sufficient to #End# the class");
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    ///  start the class and let class enter the session and clear the dictionary values for the old votes
    /// </summary>
    private void StartClass()
    {
        lock (mState.AccessLock)
        {
            // Class started
            mState.ClassInSession = true;
            VotesStart.Clear(); // Reset VotesStart for next session
            mLog.Info("Class has started.");
        }
    }

    /// <summary>
    /// End the class and reset the session state.and clear the dictionary values for the old votes
    /// </summary>
    private void EndClass()
    {
        lock (mState.AccessLock)
        {
            // Class session is over
            mState.ClassInSession = false;
            VotesEnd.Clear(); // Reset VotesStart for next session
            // Random number of students leave
            var rnd = new Random();
            int studentsLeaving = rnd.Next(1, mState.StudentCount + 1); // Ensure at least 1 student can leave if there are students
            mState.StudentCount -= studentsLeaving;
            mState.StudentCount = Math.Max(0, mState.StudentCount);
            mLog.Info($"Class has ended. {studentsLeaving} students left the classroom.");
        }
    }
}
