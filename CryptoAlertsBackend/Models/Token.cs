﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoAlertsBackend.Models
{
    [Table("Tokens")]
    public class Token
    {
        [Key]
        public int Id { get; set; }
        public string Symbol { get; set; } = "";

    }
}
