using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogWebTalkApi.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BlogWebTalkApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly BlogWebTalkApiDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ArticlesController(BlogWebTalkApiDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: api/Articles
        /// <summary>
        /// get all Articles from database
        /// </summary>
        /// <returns>list of Articles</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            return await _context.Articles.Include(c => c.Category).Select(a => new Article()
            {
                ArticleId = a.ArticleId,
                ArticleTitle = a.ArticleTitle,
                ArticleIngress = a.ArticleIngress,
                ArticlePublishDate = a.ArticlePublishDate,
                CreatedBy = a.CreatedBy,
                StickyArticle = a.StickyArticle,
                CategoryId = a.CategoryId,
                Category = a.Category,
                ArticleImageName = a.ArticleImageName,
                ArticleImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, a.ArticleImageName),
                ArticleParagraphs = a.ArticleParagraphs.Select(a => new ArticleParagraph()
                {
                    ArticleParagraphId= a.ArticleParagraphId,
                    ArticleParagraphTitle = a.ArticleParagraphTitle,
                    Content = a.Content,
                    Article = a.Article,
                    ArticleParagraphImageName = a.ArticleParagraphImageName,
                    ArticleParagraphImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, a.ArticleParagraphImageName)
                }).ToList()

            }).ToListAsync();
        }

        /// <summary>
        /// get last 3 Articles from database include articles
        /// </summary>
        /// <returns>list of 3 last Articles</returns>
        [HttpGet("GetLastArticles")]
        public async Task<ActionResult<IEnumerable<Article>>> GetLastArticles()
        {
            return await _context.Articles.OrderByDescending(a => a.ArticleId).Take(3).Include(c => c.Category).ToListAsync();

        }

        
        // GET: api/Articles/5
        /// <summary>
        /// get a article from database by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>article</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var article = await _context.Articles.Include(c => c.Category).Select(a => new Article()
            {
                ArticleId = a.ArticleId,
                ArticleTitle = a.ArticleTitle,
                ArticleIngress = a.ArticleIngress,
                ArticlePublishDate = a.ArticlePublishDate,
                CreatedBy = a.CreatedBy,
                StickyArticle = a.StickyArticle,
                CategoryId = a.CategoryId,
                Category = a.Category,
                ArticleImageName = a.ArticleImageName,
                ArticleImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, a.ArticleImageName)

            }).Where(a => a.ArticleId == id).FirstOrDefaultAsync();

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }

        [HttpGet("GetArticleDetails/{id}")]
        public async Task<ActionResult<Article>> GetArticleDetails(int id)
        {
            var article = await _context.Articles.Include(c => c.Category)
                .Where(c => c.ArticleId == id).FirstOrDefaultAsync();

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }

        // PUT: api/Articles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// update article by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="article"></param>
        /// <returns>article</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, [FromForm] Article article)
        {
            if (id != article.ArticleId)
            {
                return BadRequest();
            }
            if (article.ArticleImageFile != null)
            {
                DeleteImage(article.ArticleImageName);
                article.ArticleImageName = await SaveImage(article.ArticleImageFile);
            }

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Articles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Add new article to database
        /// </summary>
        /// <param name="article"></param>
        /// <returns>article</returns>
        [HttpPost]
        public async Task<ActionResult<Article>> PostArticle([FromForm]Article article)
        {
            article.ArticleImageName = await SaveImage(article.ArticleImageFile);
            //var model = new Article
            //{
            //    ArticleTitle = article.ArticleTitle,
            //    ArticleIngress = article.ArticleIngress,
            //    ArticlePublishDate = article.ArticlePublishDate,
            //    CreatedBy = article.CreatedBy,
            //    StickyArticle = article.StickyArticle,
            //    ArticleImageName = article.ArticleImageName,
            //    CategoryId = article.CategoryId,


            //};
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticle", new { id = article.ArticleId }, article);
        }

        // DELETE: api/Articles/5
        /// <summary>
        /// delete article by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            DeleteImage(article.ArticleImageName);
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        /// search article by id if exist in database or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns>article</returns>
        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }

        [NonAction]
        /// <summary>
        /// load image file and save it to database
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns>image name</returns>
        public async Task<string> SaveImage(IFormFile imageFile)
        {
            string imageName = new String(Path.GetFileNameWithoutExtension(imageFile.FileName).Take(10).ToArray()).Replace(' ', '-');
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(imageFile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return imageName;
        }

        /// <summary>
        /// delete image from database
        /// </summary>
        /// <param name="imageName"></param>
        [NonAction]
        public void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }
    }
}
