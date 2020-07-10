using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;


namespace AuthServer.Entities
{
    [Table("identities")]
    public class Identity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [JsonIgnore, Column("password")]
        public string HashedPassword { get; set; }
    }
}