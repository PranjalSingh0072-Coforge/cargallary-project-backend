using Microsoft.AspNetCore.Mvc;
using DapperApi.Models;
using DapperApi.Repositories;
using ClosedXML.Excel;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Drawing.Diagrams;


namespace DapperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ProductController(ProductRepository repository) : ControllerBase
    {
        private readonly ProductRepository _repository = repository;

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAll()
        {
            return await _repository.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        // [HttpPost]
        // public async Task<ActionResult<Product>> Add([FromForm] Product product, IFormFile imageFile)
        // {
        //     if (imageFile != null)
        //     {
        //         var imagePath = Path.Combine(@"D:\MyImages", imageFile.FileName);
        //         using (var stream = new FileStream(imagePath, FileMode.Create))
        //         {
        //             await imageFile.CopyToAsync(stream);
        //         }
        //         product.ImageUrl = imagePath;
        //     }
 
        //     product.CreatedDate = DateTime.Now;
        //     product.UpdateDate = DateTime.Now;

        //     var id = await _repository.AddAsync(product);
        //     product.Id = id;
        //     return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        // }

[HttpPost]
public async Task<ActionResult<Product>> Add([FromForm] Product product, IFormFile imageFile)
{
    if (imageFile != null)
    {
        // Define the path where you want to save the image
        var imagePath = Path.Combine(@"C:\Users\Pranjal.Singh\DapperApi\wwwroot\src\image", imageFile.FileName);
        
        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

        // Save the image file to the specified path
        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        // Set the ImageUrl property to the relative path
        product.ImageUrl = Path.Combine("src/image", imageFile.FileName).Replace("\\", "/");
    }

    product.CreatedDate = DateTime.Now;
    product.UpdateDate = DateTime.Now;

    var id = await _repository.AddAsync(product);
    product.Id = id;
    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
}

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product, IFormFile imageFile)
        {
             if (product == null || product.Id != id)
            {
                return BadRequest();
            }

            if (imageFile != null)
            {
                var imagePath = Path.Combine(@"D:\MyImages", imageFile.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ImageUrl = imagePath;
            }

            await _repository.UpdateAsync(product);
            // return NoContent();
            
    return Ok(new { result = true });
        }

        // For PUT
      



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }



        [HttpGet("download")]

        public async Task<IActionResult> DownloadExcel()
        {
            var products = await _repository.GetAllAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Price";
                worksheet.Cell(1, 4).Value = "ImageUrl";
                worksheet.Cell(1, 5).Value = "Specifications";
                worksheet.Cell(1, 6).Value = "Quantity";

                var row = 2;
                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.Id;
                    worksheet.Cell(row, 2).Value = product.Name;
                    worksheet.Cell(row, 3).Value = product.Price;
                    worksheet.Cell(row, 4).Value = product.ImageUrl;
                    worksheet.Cell(row, 5).Value = product.Specifications;
                    worksheet.Cell(row, 6).Value = product.Quantity;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
                }

            }
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var Products = new List<Product>();
            try
            {

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var Product = new Product
                            {
                                Id = int.Parse(worksheet.Cells[row, 1]?.Value?.ToString() ?? "0"),
                                Name = worksheet.Cells[row, 2]?.Value?.ToString() ?? string.Empty,
                                Price = decimal.Parse(worksheet.Cells[row, 3]?.Value?.ToString() ?? "0"),
                                Quantity = int.Parse(worksheet.Cells[row, 4]?.Value?.ToString() ?? "0"),
                                ImageUrl = worksheet.Cells[row, 5]?.Value?.ToString() ?? string.Empty,
                                Specifications = worksheet.Cells[row, 6]?.Value?.ToString() ?? string.Empty
                            };


                            Products.Add(Product);
                        }
                    }
                }
                await _repository.SaveData(Products);
                return Ok("File successfully uploaded and data saved to database.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}