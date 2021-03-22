using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xamarin.Forms;

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
        
        public string Address { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public string FileName { get; set; }
        
        [Required]
        public string Origin { get; set; }
    }
}
