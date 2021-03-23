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
    public class ArticleParagraphsController : ControllerBase
    {
        private readonly BlogWebTalkApiDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ArticleParagraphsController(BlogWebTalkApiDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: api/ArticleParagraphs

        /// <summary>
        /// get all ArticleParagraphs from database
        /// </summary>
        /// <returns>list of ArticleParagraphs</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleParagraph>>> GetArticleParagraphs()
        {
            return await _context.ArticleParagraphs.Include(a => a.Article).Select(p => new ArticleParagraph()
            {
                ArticleParagraphId = p.ArticleParagraphId,
                ArticleParagraphTitle = p.ArticleParagraphTitle,
                Content = p.Content,
                ArticleId = p.ArticleId,
                Article = p.Article,
                ArticleParagraphImageName = p.ArticleParagraphImageName,
                ArticleParagraphImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, p.ArticleParagraphImageName)
            }).ToListAsync();
        }
        
        // GET: api/ArticleParagraphs/5
        /// <summary>
        /// get articleparagraph by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleParagraph>> GetArticleParagraph(int id)
        {
            var articleParagraph = await _context.ArticleParagraphs.FindAsync(id);

            if (articleParagraph == null)
            {
                return NotFound();
            }

            return articleParagraph;
        }
        [HttpGet("GetArticleParagraphDetails/{id}")]
        public async Task<ActionResult<ArticleParagraph>> GetArticleParagraphDetails(int id)
        {
            var articleParagraph = await _context.ArticleParagraphs.Include(c => c.Article)
                .Where(c => c.ArticleParagraphId == id).FirstOrDefaultAsync();

            if (articleParagraph == null)
            {
                return NotFound();
            }

            return articleParagraph;
        }

        // PUT: api/ArticleParagraphs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// update by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="articleParagraph"></param>
        /// <returns>articleParagraph</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticleParagraph(int id,[FromForm] ArticleParagraph articleParagraph)
        {
            if (id != articleParagraph.ArticleParagraphId)
            {
                return BadRequest();
            }
            if (articleParagraph.ArticleParagraphImageFile != null)
            {
                DeleteImage(articleParagraph.ArticleParagraphImageName);
                articleParagraph.ArticleParagraphImageName = await SaveImage(articleParagraph.ArticleParagraphImageFile);
            }

            _context.Entry(articleParagraph).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleParagraphExists(id))
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

        // POST: api/ArticleParagraphs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// add new articlepagraph to database
        /// </summary>
        /// <param name="articleParagraph"></param>
        /// <returns>articleParagraph</returns>
        [HttpPost]
        public async Task<ActionResult<ArticleParagraph>> PostArticleParagraph([FromForm] ArticleParagraph articleParagraph)
        {
            articleParagraph.ArticleParagraphImageName = await SaveImage(articleParagraph.ArticleParagraphImageFile);
            _context.ArticleParagraphs.Add(articleParagraph);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticleParagraph", new { id = articleParagraph.ArticleParagraphId }, articleParagraph);
        }

        // DELETE: api/ArticleParagraphs/5
        /// <summary>
        /// delete articleparagraph by id from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticleParagraph(int id)
        {
            var articleParagraph = await _context.ArticleParagraphs.FindAsync(id);
            if (articleParagraph == null)
            {
                return NotFound();
            }
            DeleteImage(articleParagraph.ArticleParagraphImageName);
            _context.ArticleParagraphs.Remove(articleParagraph);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArticleParagraphExists(int id)
        {
            return _context.ArticleParagraphs.Any(e => e.ArticleParagraphId == id);
        }
        [NonAction]
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

        [NonAction]
        public void DeleteImage(string imageName)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }
    }
}
