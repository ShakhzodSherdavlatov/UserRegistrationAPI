using System.ComponentModel.DataAnnotations;

namespace UserRegistrationAPI.API.DataContracts
{
    /// <summary>
    /// User
    /// </summary>
    public class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string givenName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string sn { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string telephoneNumber { get; set; }

    }
}
