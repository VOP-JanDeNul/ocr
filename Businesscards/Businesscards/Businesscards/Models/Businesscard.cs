using SQLite;
using System;
using System.ComponentModel.DataAnnotations;

namespace Businesscards.Models
{
    public class Businesscard
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Company { get; set; }

        [Required]
        public string Name { get; set; }

        // ???
        public string Nature { get; set; }

        public string Jobtitle { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Phone]
        public string Mobile { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Fax { get; set; }

        // Street + Number
        public string Street { get; set; }
        // City + PostalCode
        public string City { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // base64 version of the image
        public string Base64 { get; set; }

        // Filled in by user to identify who sends the businesscard
        [Required]
        public string Origin { get; set; }

        // Extra information we can't put into a specific field
        public string Extra { get; set; }
    }
}
