namespace InmateSearchWebApp.Models
{
    public class AccessCorrectionsOptions
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = "StrongPassword123";
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = "Smith";
        public string Phone { get; set; } = "5551234567";
        public string Address { get; set; } = string.Empty;
        public string MiddleName { get; set; } = "sumit";
        public string DateOfBirth { get; set; } = "02/12/1994";
        public string State { get; set; } = "Texas";
        public string City { get; set; } = "Dallas";
        public string Zip { get; set; } = "471642";
        public string Agency { get; set; } = "Tarrant County Jail";
    }
}
