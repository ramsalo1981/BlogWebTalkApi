using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BlogWebTalkApi.Models
{
    public partial class Article
    {
        public Article()
        {
            ArticleParagraphs = new HashSet<ArticleParagraph>();
        }

        public int ArticleId { get; set; }
        public string ArticleTitle { get; set; }
        public string ArticleIngress { get; set; }
        public string ArticleImageName { get; set; }
        public DateTime? ArticlePublishDate { get; set; }
        public string CreatedBy { get; set; }
        public bool StickyArticle { get; set; }
        public int CategoryId { get; set; }
        [NotMapped]
        public IFormFile ArticleImageFile { get; set; }

        [NotMapped]
        public string ArticleImageSrc { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ArticleParagraph> ArticleParagraphs { get; set; }
    }
}
