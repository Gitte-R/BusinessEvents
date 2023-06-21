using BusinessEvents.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;

namespace BusinessEvents
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<BusinessEvent> NewListOfBusinessEvents = new List<BusinessEvent>();

        public MainWindow()
        {
            InitializeComponent();
            ReadHardcodeData();
        }

        private void ReadHardcodeData()
        {
            BusinessEvent NewBusinessEvent1 = new BusinessEvent("31/12/2023", "Nytårsaften", 50, "Nytårsaften i Tivoli.", "09/12/2023");
            BusinessEvent NewBusinessEvent2 = new BusinessEvent("24/09/2020", "Biotur for 2", 2, "Biotur i Palads", "20/09/2020");
            BusinessEvent NewBusinessEvent3 = new BusinessEvent("01/05/2022", "1. Maj", 200, "1. Maj i Fælledparken", "25/04/2022");
            BusinessEvent NewBusinessEvent4 = new BusinessEvent("15/07/2023", "Skovtur i Dyrehaven", 2, "Picnic for to.", "01/07/2023");
            NewBusinessEvent1.OnBusinessEventClosedOrFull += BusinessEventClosedOrFull;
            NewBusinessEvent2.OnBusinessEventClosedOrFull += BusinessEventClosedOrFull;
            NewBusinessEvent3.OnBusinessEventClosedOrFull += BusinessEventClosedOrFull;
            NewBusinessEvent4.OnBusinessEventClosedOrFull += BusinessEventClosedOrFull;
            NewListOfBusinessEvents.Add(NewBusinessEvent1);
            NewListOfBusinessEvents.Add(NewBusinessEvent2);
            NewListOfBusinessEvents.Add(NewBusinessEvent3);
            NewListOfBusinessEvents.Add(NewBusinessEvent4);
            Participant Newparticipant1 = new Participant("Peter Petersen", "peter@petersen.dk");
            Participant Newparticipant2 = new Participant("Frederik Frederiksen", "frederik@frederiksen.dk");
            Participant Newparticipant3 = new Participant("Hanne Hansen", "hanne@hansen.dk");
            Participant Newparticipant4 = new Participant("Jens Jensen", "jens@jensen.dk");
            NewBusinessEvent2.ListOfParticipants.Add(Newparticipant1);
            NewBusinessEvent2.ListOfParticipants.Add(Newparticipant2);
            NewBusinessEvent4.ListOfParticipants.Add(Newparticipant3);
            NewBusinessEvent4.ListOfParticipants.Add(Newparticipant4);
        }

        #region Methods for navigating program and views
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBusinessEventStatus();

            if (TabItemPreviousBusinessEvents.IsSelected)
            { 
                //Show all previous BusinessEvents when PreviousBusinessEvent Tab is selected and update the datagrid if new BusinessEvents is created.
                UpdatePreviousBusinessEventsDataGrid();
            }
            else if (TabItemSignUpForBusinessEvents.IsSelected)
            {
                //Only shows open BusinessEvents in dropdown menu. This is BusinessEvents that have not surpassed deadline for registration and have not reached max participants. 
                DropDownOpenBusinessEvents.ItemsSource = FindOpenBusinessEvents();
            }
        }
        private void RadioBtnPreviousBusinessEvents_Checked(object sender, RoutedEventArgs e)
        {
            ShowPreviousBusinessEvents();
        }
        private void RadioBtnClosedBusinessEvents_Checked(object sender, RoutedEventArgs e)
        {
            ShowClosedBusinessEvents();
        }
        private void RadioBtnFullyBookedBusinessEvents_Checked(object sender, RoutedEventArgs e)
        {
            ShowFullyBookedBusinessEvents();
        }
        private void UpdatePreviousBusinessEventsDataGrid()
        {
            if (RadioBtnPreviousBusinessEvents.IsChecked == true)
            {
                ShowPreviousBusinessEvents();
            }
            else if (RadioBtnClosedBusinessEvents.IsChecked == true)
            {
                ShowClosedBusinessEvents();
            }
            else if (RadioBtnFullyBookedBusinessEvents.IsChecked == true)
            {
                ShowFullyBookedBusinessEvents();
            }
        }
        private void ShowPreviousBusinessEvents()
        {
            PreviousBusinessEventsDataGrid.Items.Clear();
            foreach (BusinessEvent businessEvent in NewListOfBusinessEvents)
                if (businessEvent.CurrentStatus != BusinessEvent.Status.Open)
                {
                    PreviousBusinessEventsDataGrid.Items.Add(businessEvent);
                }
        }
        private void ShowClosedBusinessEvents()
        {
            PreviousBusinessEventsDataGrid.Items.Clear();
            foreach (BusinessEvent businessEvent in NewListOfBusinessEvents)
                if (businessEvent.CurrentStatus == BusinessEvent.Status.Closed)
                {
                    PreviousBusinessEventsDataGrid.Items.Add(businessEvent);
                }
        }
        private void ShowFullyBookedBusinessEvents()
        {
            PreviousBusinessEventsDataGrid.Items.Clear();
            foreach (BusinessEvent businessEvent in NewListOfBusinessEvents)
                if (businessEvent.CurrentStatus == BusinessEvent.Status.Full)
                {
                    PreviousBusinessEventsDataGrid.Items.Add(businessEvent);
                }
        }
        #endregion

        #region Methods for creating BusinessEvents
        private void Create_BusinessEvent_Click(object sender, RoutedEventArgs e)
        {
            //If input from user is not a number higher than 0, TryParse vil leave IntMaxParticipants to 0. In this case an instance of BusinessEvent will be created with a default value of maximum participants. 
            int.TryParse(TxtMaxParticipants.Text, out int IntMaxParticipants);

            if (ValidateBusinessEventInput() && ValidatCalenderInput() && CheckNameForDuplicates())
            {
                switch (IntMaxParticipants)
                {
                    case > 0:
                        BusinessEvent NewBusinessEvent1 = new BusinessEvent(CalDateOfBusinessEvent.Text, TxtNameOfBusinessEvent.Text.Trim(), IntMaxParticipants, TxtBusinessEventDescription.Text, CalRegistrationDeadline.Text);
                        NewListOfBusinessEvents.Add(NewBusinessEvent1);
                        NewBusinessEvent1.OnBusinessEventClosedOrFull += BusinessEventClosedOrFull;
                        ClearCreateBusinessEventInput();
                        break;
                    default:
                        BusinessEvent NewBusinessEvent2 = new BusinessEvent(CalDateOfBusinessEvent.Text, TxtNameOfBusinessEvent.Text.Trim(), TxtBusinessEventDescription.Text, CalRegistrationDeadline.Text);
                        NewListOfBusinessEvents.Add(NewBusinessEvent2);
                        NewBusinessEvent2.OnBusinessEventClosedOrFull += BusinessEventClosedOrFull;
                        ClearCreateBusinessEventInput();
                        break;
                }    
            }  
        }
        private bool ValidateBusinessEventInput()
        {
            bool NameHasInput = !String.IsNullOrWhiteSpace(TxtNameOfBusinessEvent.Text);
            bool DescriptionHasInput = !String.IsNullOrWhiteSpace(TxtBusinessEventDescription.Text);
            bool DateHasInput = CalDateOfBusinessEvent.SelectedDate.HasValue;
            bool DeadlineHasInput = CalRegistrationDeadline.SelectedDate.HasValue;

            if (NameHasInput && DescriptionHasInput && DateHasInput && DeadlineHasInput)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Input is missing");
                return false;
            }
        }
        private bool CheckNameForDuplicates()
        {
            List<string> ListOfNames = new List<string>();

            foreach (BusinessEvent businessEvent in FindOpenBusinessEvents())
            {
                ListOfNames.Add(businessEvent.NameOfBusinessEvent);
            }

            if (ListOfNames.Contains(TxtNameOfBusinessEvent.Text.Trim()))
            {
                MessageBox.Show("Name of BusinessEvent is already in use");
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool ValidatCalenderInput()
        {
            if (CalDateOfBusinessEvent.SelectedDate.Value.Date <= DateTime.Now.Date)
            {
                MessageBox.Show("Date of BusinessEvent must not be later than tomorrow");
                return false;
            }
            if (CalRegistrationDeadline.SelectedDate.Value.Date < DateTime.Now.Date)
            {
                MessageBox.Show("Deadline of registration must not be later than today");
                return false;
            }
            if (CalRegistrationDeadline.SelectedDate.Value.Date > CalDateOfBusinessEvent.SelectedDate.Value.Date)
            {
                MessageBox.Show("Date of registration must be prior to date of BusinessEvent");
            }
            return true;
        }

        /// <summary>
        /// Clears the textboxes and reset calenders when a BusinessEvent is created.
        /// </summary>
        private void ClearCreateBusinessEventInput()
        {
            TxtNameOfBusinessEvent.Clear();
            TxtMaxParticipants.Clear();
            TxtBusinessEventDescription.Clear();
            CalDateOfBusinessEvent.SelectedDate = DateTime.Today;
            CalRegistrationDeadline.SelectedDate = DateTime.Today;
        }
        #endregion

        #region Methods for signing up to BusinessEvents
        private void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSignUpInput())
            {
                //Cast the selected BusinessEvent in the dropdown menu to the BusinessEvent type.
                BusinessEvent SelectedBusinessEvent = (BusinessEvent)DropDownOpenBusinessEvents.SelectedItem;
                Participant NewParticipant = new Participant(TxtNameOfParticipant.Text.Trim(), TxtEmailOfParticipant.Text.Trim());
                SelectedBusinessEvent.ListOfParticipants.Add(NewParticipant);
                ClearSignUpInput();
                UpdateBusinessEventStatus();
            }
            else
            {
                MessageBox.Show("Your input is not valid");
            }
        }
        private void DropDownOpenBusinessEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindOpenBusinessEvents() is not null)
            {
                SignUpBtn.IsEnabled = true;
            }
        }
        private List<BusinessEvent> FindOpenBusinessEvents()
        {
            List<BusinessEvent> ListOfOpenBusinessEvents = new List<BusinessEvent>();

            foreach (BusinessEvent businessEvent in NewListOfBusinessEvents)
            {
                if (businessEvent.CurrentStatus == BusinessEvent.Status.Open)
                {
                    ListOfOpenBusinessEvents.Add(businessEvent);
                }
            }
            return ListOfOpenBusinessEvents;
        }
        private bool ValidateSignUpInput()
        {
            if (!string.IsNullOrWhiteSpace(TxtNameOfParticipant.Text.Trim()) && ValidateEmail())
            {
                return true;
            }
            else
            {
                return false;
            }    
        }
        private bool ValidateEmail()
        {
            try
            {
                MailAddress UserEmail = new MailAddress(TxtEmailOfParticipant.Text.Trim());
                return true;
            }
            catch (ArgumentNullException ex) { MessageBox.Show(ex.Message); }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message); }
            catch (FormatException ex) { MessageBox.Show(ex.Message); }

            return false;
        }

        /// <summary>
        /// Clears the textboxes and reset dropdown selection when signing up to event.
        /// </summary>
        private void ClearSignUpInput()
        {
            TxtNameOfParticipant.Text = string.Empty;
            TxtEmailOfParticipant.Text = string.Empty;
            DropDownOpenBusinessEvents.SelectedIndex = -1;
            SignUpBtn.IsEnabled = false;
        }
        #endregion

        #region Methods for managing status of Business Event and lists of participants
        private void UpdateBusinessEventStatus()
        {
            foreach (BusinessEvent businessEvent in FindOpenBusinessEvents())
            {
                DateTime.TryParse(businessEvent.DateOfBusinessEvent, out DateTime DateTimeOfBusinessEvent);
                DateTime.TryParse(businessEvent.RegistrationDeadline, out DateTime DateTimeDeadline);

                //The Closed status is higher than the Full status.
                if (businessEvent.ListOfParticipants.Count == businessEvent.MaxParticipants)
                {
                    businessEvent.CurrentStatus = BusinessEvent.Status.Full;
                    businessEvent.BusinessEventNotOpen(businessEvent);
                }
                if (DateTimeOfBusinessEvent < DateTime.Today || DateTimeDeadline < DateTime.Today)
                {
                    businessEvent.CurrentStatus = BusinessEvent.Status.Closed;
                    businessEvent.BusinessEventNotOpen(businessEvent);
                }
            }
        }
        private void BusinessEventClosedOrFull(object sender, EventArgs e)
        {
            CreateDocumentOfParticipants(sender as BusinessEvent);
        }
        private void CreateDocumentOfParticipants(BusinessEvent businessEvent)
        {
            try
            {
                string _jsonFileName = @"C:\Users\gitte\Documents\" + businessEvent.NameOfBusinessEvent + ".json";
                string serialized = JsonConvert.SerializeObject(businessEvent.ListOfParticipants, Formatting.Indented);
                File.WriteAllText(_jsonFileName, serialized);
            }
            catch (DirectoryNotFoundException ex) { MessageBox.Show(ex.Message); }
            catch (PathTooLongException ex) { MessageBox.Show(ex.Message); }
            catch (ArgumentNullException ex) { MessageBox.Show(ex.Message); }
            catch (FormatException ex) { MessageBox.Show(ex.Message); }
        }
        #endregion
    }
}

