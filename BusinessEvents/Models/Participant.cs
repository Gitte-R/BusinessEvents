namespace BusinessEvents.Models
{
    public class Participant
    {
        public string Name { get; init; }
        public string Email { get; init; }

        public Participant(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
