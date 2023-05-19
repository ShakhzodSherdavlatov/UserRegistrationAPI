using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DbTools.Entities.Abstract;

namespace UserRegistrationAPI.API.DataContracts
{
    /// <summary>
    /// User
    /// </summary>
    [Table("User")]
    public class User : Entity
    { 
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
