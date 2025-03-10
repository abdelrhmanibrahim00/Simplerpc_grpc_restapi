namespace Servers;
using Services;
using System.Collections.Generic;

/// <summary>
/// Classroom service that handles door , students and teacher votes.
/// </summary>
public class ClassroomService : IClassroomService
{
    /// <summary>
   /// Represents the logic that handles classroom-related operations.
   /// </summary>
    private ClassroomLogic classroomLogic = new ClassroomLogic();

    /// <summary>
    /// Check if there is enough students has been sent to the class
    /// </summary>
    public bool enoughstudents()
    {
        return classroomLogic.canvotingstart();
    }
    
    /// <summary>
    /// Get next unique ID from the server.
    /// </summary>
    /// <returns>Unique ID.</returns>
    public int GetUniqueId()
    {
        return classroomLogic.GetUniqueId();
    }

    /// <summary>
    /// Pass the amount of generated students from door client class
    /// </summary>
    public void Generatednumberofstudnent(DoorDesc door)
    {
        classroomLogic.studnetnsnumber(door.AmountOfStudents);
    }

    /// <summary>
    /// Get current classroom session state.
    /// </summary>
    /// <returns>True if class is in session, false otherwise.</returns>
    public bool IsClassInSession()
    {
        return classroomLogic.IsClassInSession();
    }

    /// <summary>
    /// Evaluate the teacher votes for starting the class through an object of teacher
    /// </summary>
    public bool VoteStartClass(Teacher teacher)
    {
        return classroomLogic.VoteToStartClass(teacher);
    }

    /// <summary>
    /// Evaluate the teacher votes for starting the class through an object of teacher
    /// </summary>
    public bool VoteEndClass(Teacher teacher)
    {
        return classroomLogic.VoteToEndClass(teacher);
    }

    /// <summary>
    /// Start the class process.
    /// </summary>
    public bool StartClass(Teacher teacher)
    {
        return classroomLogic.VoteToStartClass(teacher); // Public method to start class
    }

}
