using System.Text.Json.Serialization;

namespace ECO.BLL.DTO.Order
{
    public class ShippingAddressDto
    {
        public string FirstName { get; set; }

        // Temporary backward compatibility: old clients still send/read "fristName" (the old misspelling). Remove once no old client remains.
        [JsonPropertyName("fristName")]
        public string FirstNameLegacy
        {
            get => FirstName;
            set => FirstName = value;
        }
        public string LastName { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}