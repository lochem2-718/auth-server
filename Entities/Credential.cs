using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;


namespace AuthServer.Entities
{
    public class Credential
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        public string Username { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}