using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BlogWebTalkApi.Models
{
    public partial class Category
    {
        public Category()
        {
            Articles = new HashSet<Article>();
        }

        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
        public string CategoryImageName { get; set; }
        public DateTime? CategoryPublishDate { get; set; }
        [NotMapped]
        public IFormFile CategoryImageFile { get; set; }

        [NotMapped]
        public string CategoryImageSrc { get; set; }
        public virtual ICollection<Article> Articles { get; set; }
    }
}
