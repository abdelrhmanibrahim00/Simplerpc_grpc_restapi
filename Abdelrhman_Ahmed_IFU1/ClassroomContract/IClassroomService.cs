using System.Reflection.Metadata;

namespace Services
{
    /// <summary>
    /// Door descriptor.
    /// </summary>
    public class DoorDesc
    {
        /// <summary>
        /// Unique ID for the door.
        /// </summary>
        public int DoorId { get; set; }

        /// <summary>
        /// Number of students currently at this door.
        /// </summary>
        public int AmountOfStudents { get; set; }

        /// <summary>
        /// Indicates if the door is closed.
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Indicates if the door is opened.
        /// </summary>
        public bool IsOpened { get; set; }

        /// <summary>
        /// Door description (optional).
        /// </summary>
        public string Description { get; set; }

     /// <summary>
     /// Initializes a new instance of the <see cref="DoorDesc"/> class with the specified parameters.
    /// </summary>
    /// <param name="doorId">The unique identifier for the door.</param>
    /// <param name="amountOfStudents">The number of students associated with the door.</param>
    /// <param name="isClosed">Indicates whether the door is closed.</param>
    /// <param name="isOpened">Indicates whether the door is opened.</param>
    /// <param name="description">A description of the door.</param>
        public DoorDesc(int doorId, int amountOfStudents, bool isClosed, bool isOpened, string description)
        {
            DoorId = doorId;
            AmountOfStudents = amountOfStudents;
            IsClosed = isClosed;
            IsOpened = isOpened;
            Description = description;
        }
    }

    /// <summary>
    /// Teacher descriptor.
    /// </summary>
    public class Teacher
    {
        /// <summary>
        /// Unique ID for the teacher.
        /// </summary>
        public int TeacherId { get; set; }

        /// <summary>
        /// Indicates whether the teacher has already voted to start the class.
        /// </summary>
        public bool HasVotedToStart { get; set; }

        /// <summary>
        /// Indicates whether the teacher has already voted to end the class.
        /// </summary>
        public bool HasVotedToEnd { get; set; }

        /// <summary>
        /// Teacher description (optional).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
       /// Initializes a new instance of the <see cref="Teacher"/> class.
       /// </summary>
        public Teacher()
        {

        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="Teacher"/> class with the specified parameters.
        /// </summary>
        /// <param name="teacherId">The unique identifier for the teacher.</param>
       /// <param name="hasVotedToStart">Indicates whether the teacher has voted to start the class.</param>
       /// <param name="hasVotedToEnd">Indicates whether the teacher has voted to end the class.</param>
       /// <param name="Name">The name of the teacher.</param>
        public Teacher(int teacherId, bool hasVotedToStart, bool hasVotedToEnd, string Name)
        {
            this.TeacherId = teacherId;
            this.HasVotedToStart = hasVotedToStart;
            this.HasVotedToEnd = hasVotedToEnd;
            this.Name = Name;
        }
    }

    /// <summary>
    /// Service contract Interface .
    /// </summary>
    public interface IClassroomService
    {
        /// <summary>
        /// Get next unique ID from the server. Is used by doors or teachers to acquire IDs.
        /// </summary>
        /// <returns>Unique ID.</returns>
        int GetUniqueId();

        /// <summary>
        /// Send the votes from teacher client to class to the server to be evaluated there 
        /// </summary>
        /// <returns> a boolean true or false if the votes were enough to start the class</returns>
        bool StartClass(Teacher teacher);

        /// <summary>
        /// Send the number of generated students to the server 
        /// </summary>
        void Generatednumberofstudnent(DoorDesc door);

        /// <summary>
        /// Check if there are enough students before teachers start voting
        /// </summary>
        bool enoughstudents();

        /// <summary>
        /// Get current classroom session state.
        /// </summary>
        /// <returns>True if class is in session, false otherwise.</returns>				
        bool IsClassInSession();

        /// <summary>
        /// Send the teacher votes
        /// </summary>
        /// <returns> true if the votes enough to start a class </returns>	
        bool VoteStartClass(Teacher teacher);

        /// <summary>
        /// Send the teacher votes
        /// </summary>
        /// <returns> true if the votes enough to End a class </returns>
        bool VoteEndClass(Teacher teacher);
    }
}
