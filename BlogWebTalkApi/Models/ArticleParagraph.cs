using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BlogWebTalkApi.Models
{
    public partial class ArticleParagraph
    {
        public int ArticleParagraphId { get; set; }
        public string ArticleParagraphTitle { get; set; }
        public string ArticleParagraphImageName { get; set; }
        public string Content { get; set; }
        public int ArticleId { get; set; }
        [NotMapped]
        public IFormFile ArticleParagraphImageFile { get; set; }

        [NotMapped]
        public string ArticleParagraphImageSrc { get; set; }
        public virtual Article Article { get; set; }
    }
}
