using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P225FirstApi.Data;
using P225FirstApi.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P225FirstApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Category category)
        {
            if (category.IsMain)
            {
                if (category.Image == null) return BadRequest("Image Is Required");
            }
            else
            {
                if (category.ParentId == null) return BadRequest("Parent Id Is Required");

                if(!await _context.Categories.AnyAsync(c=>!c.IsDeleted && c.IsMain && c.Id == category.ParentId)) return BadRequest("Parent Id Is InCorrect");

                if(await _context.Categories.AnyAsync(c=>!c.IsDeleted && c.Name.ToLower() == category.Name.Trim().ToLower())) 
                    return BadRequest($"Category Name = {category.Name} Is Alredy Exists");
            }

            category.Name = category.Name.Trim();
            category.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, category);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Categories.Where(c => !c.IsDeleted).ToListAsync());
        }

        [HttpGet]
        [Route("{id?}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id == null) return BadRequest("Id Is Required");

            Category category = await _context.Categories.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (category == null) return NotFound();

            return Ok(category);
        }

        [HttpPut]
        [Route("{id?}")]
        public async Task<IActionResult> Put(int? id, Category category)
        {
            if (id == null) return BadRequest("Id Is Requeired");

            if (id != category.Id) return BadRequest("Id Is Not Macthed");

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (dbCategory == null) return NotFound("Id Is InCorrect");

            if (category.IsMain)
            {
                if (category.Image == null) return BadRequest("Image Is Required");
            }
            else
            {
                if (category.ParentId == null) return BadRequest("Parent Id IsRequired");

                if (category.ParentId == id) return BadRequest("Id and Parent Is Same");

                if (!await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == category.ParentId)) return BadRequest("Parent Id Is InCorrect");
            }

            if (await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id != id && c.Name.ToLower() == category.Name.Trim().ToLower()))
                return Conflict("Category Name Alreade Exists");

            dbCategory.Name = category.Name.Trim();
            dbCategory.IsMain = category.IsMain;
            dbCategory.ParentId = category.ParentId;
            dbCategory.Image = category.Image;
            dbCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        [Route("{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest("Id Is Requeired");

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);

            if (dbCategory == null) return NotFound("Id Is InCorrect");

            dbCategory.IsDeleted = true;
            dbCategory.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
