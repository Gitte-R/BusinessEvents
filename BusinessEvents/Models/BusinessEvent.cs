using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessEvents.Models
{
    public partial class BusinessEvent
    {
        public string DateOfBusinessEvent { get; init; }
        public string NameOfBusinessEvent { get; init; }
        public int MaxParticipants { get; init; } = 1000000;
        public Status CurrentStatus { get; set; }
        public string BusinessEventDescription { get; init; }
        public string RegistrationDeadline { get; init; }
        public List<Participant> ListOfParticipants { get; init; }

        public event EventHandler OnBusinessEventClosedOrFull;

        public BusinessEvent(string dateOfBusinessEvent, string nameOfBusinessEvent, string businessEventDescription, string registrationDeadline)
        {
            DateOfBusinessEvent = dateOfBusinessEvent;
            NameOfBusinessEvent = nameOfBusinessEvent;
            BusinessEventDescription = businessEventDescription;
            RegistrationDeadline = registrationDeadline;
            CurrentStatus = Status.Open; //All event are open, as default, when created.
            ListOfParticipants = new List<Participant>();
        }

        public BusinessEvent(string dateOfBusinessEvent, string nameOfBusinessEvent, int maxParticipants, string businessEventDescription, string registrationDeadline)
        {
            DateOfBusinessEvent = dateOfBusinessEvent;
            NameOfBusinessEvent = nameOfBusinessEvent;
            MaxParticipants = maxParticipants;
            BusinessEventDescription = businessEventDescription;
            RegistrationDeadline = registrationDeadline;
            CurrentStatus = Status.Open; //All event are open, as default, when created.
            ListOfParticipants = new List<Participant>();
        }

        public int NumberOfParticipants
        {
            get { return ListOfParticipants.Count(); }
            set { NumberOfParticipants = ListOfParticipants.Count(); }
        }

        public void BusinessEventNotOpen(BusinessEvent businessEvent)
        {
            OnBusinessEventClosedOrFull?.Invoke(businessEvent, EventArgs.Empty);
        }
    }
}

