﻿using E_Games.Data.Data.Enums;

namespace E_Games.Web.ViewModels
{
    public class CreateProducModel
    {
        public string? Name { get; set; }

        public IFormFile? Logo { get; set; }

        public IFormFile? Background { get; set; }

        public Platforms Platform { get; set; }

        public DateTime DateCreated { get; set; }

        public int TotalRating { get; set; }

        public string? Genre { get; set; }
    }
}