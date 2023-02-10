using DbTools.Entities.Abstract;

namespace UserRegistrationAPI.Services.Model
{
    public class User:Entity
    {
        public int UserId { get { return Id; } }
        public string givenName { get; set; }
        public string sn { get; set; }
        public string telephoneNumber { get; set; }

    }
}
