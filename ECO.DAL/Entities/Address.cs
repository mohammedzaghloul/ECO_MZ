using System.ComponentModel.DataAnnotations.Schema;

namespace ECO.DAL.Entities
{
    public class Address:BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey(nameof(ApplicationUserId))]
        public virtual ApplicationUser  ApplicationUser { get; set; }
    }
}