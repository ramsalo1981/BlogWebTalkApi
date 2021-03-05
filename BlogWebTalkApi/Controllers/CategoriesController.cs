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
    public class CategoriesController : ControllerBase
    {
        private readonly BlogWebTalkApiDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CategoriesController(BlogWebTalkApiDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: api/Categories
        /// <summary>
        /// get all categories from database
        /// </summary>
        /// <returns>list of categories</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.Select(c => new Category()
            {
                CategoryId = c.CategoryId,
                CategoryTitle = c.CategoryTitle,
                CategoryPublishDate = c.CategoryPublishDate,
                CategoryImageName = c.CategoryImageName,
                CategoryImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, c.CategoryImageName)

            }).ToListAsync();
        }
        /// <summary>
        /// get last 4 categories from database include last 3 articles
        /// </summary>
        /// <returns>list of 4 last categories</returns>
        [HttpGet("GetLastCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetLastCategories()
        {
            return await _context.Categories.OrderByDescending(c => c.CategoryId).Take(4)
                .Include(c => c.Articles.OrderByDescending(a => a.ArticleId).Take(3)).ToListAsync();

        }
        // GET: api/Categories/5
        /// <summary>
        /// get one category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>category</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.Select(c => new Category()
            {
                CategoryId = c.CategoryId,
                CategoryTitle = c.CategoryTitle,
                CategoryPublishDate = c.CategoryPublishDate,
                CategoryImageName = c.CategoryImageName,
                CategoryImageSrc = String.Format("{0}://{1}{2}/Images/{3}", Request.Scheme, Request.Host, Request.PathBase, c.CategoryImageName)

            }).Where(c => c.CategoryId == id).FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }
        [HttpGet("GetCategoryDetails/{id}")]
        public async Task<ActionResult<Category>> GetCategoryDetails(int id)
        {
            var category = await _context.Categories.Include(a => a.Articles)
                .Where(c => c.CategoryId == id).FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// update category by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        /// <returns>category</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromForm] Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest();
            }
            if (category.CategoryImageFile != null)
            {
                DeleteImage(category.CategoryImageName);
                category.CategoryImageName = await SaveImage(category.CategoryImageFile);
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// create new category
        /// </summary>
        /// <param name="category"></param>
        /// <returns>category</returns>
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory([FromForm] Category category)
        {
            category.CategoryImageName = await SaveImage(category.CategoryImageFile);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, category);
        }

        // DELETE: api/Categories/5
        /// <summary>
        /// delete a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            DeleteImage(category.CategoryImageName);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        /// search category by id if exist in database or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns>category</returns>
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
        /// <summary>
        /// load image file and save it to database
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns>image name</returns>
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
