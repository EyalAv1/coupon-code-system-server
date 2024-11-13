namespace CouponsCodeSystemServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string CompanyName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required bool IsAdmin { get; set; }
    }
}