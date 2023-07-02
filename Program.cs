using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Image
{
    public string Id { get; set; }
    public string? Title { get; set; }
    public string? ImagePath { get; set; }

    public Image()
    {
        Id = Guid.NewGuid().ToString();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", async (context) =>
        {
            var html = @"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""utf-8"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Image Uploader</title>
                </head>
                <body>
                    <h1>Image Uploader</h1>
                    <form action=""/"" method=""POST"" enctype=""multipart/form-data"">
                        <label for=""title"">Title:</label>
                        <input type=""text"" id=""title"" name=""title"" required><br><br>
                        <label for=""image"">Attach Image:</label>
                        <input type=""file"" id=""image"" name=""image"" accept=""image/jpeg, image/png, image/gif"" required><br><br>
                        <input type=""submit"" value=""Upload"">
                    </form>
                </body>
                </html>";

            await context.Response.WriteAsync(html);
        });

        app.MapPost("/", async (HttpContext context) =>
        {
            IFormCollection form = await context.Request.ReadFormAsync();
            string? imageTitle = form["title"];
            if (string.IsNullOrEmpty(imageTitle))
            {
                return Results.BadRequest("Empty title string");
            }
            if (form.Files.Count == 0)
            {
                return Results.BadRequest("No file uploaded");
            }
            IFormFile imageFormFile = form.Files[0];
            string fileName = imageFormFile.FileName;
            string fileExtension = fileName.Split('.')[1];
            if (fileExtension.ToLower() != "png" && fileExtension.ToLower() != "jpg" && fileExtension.ToLower() != "gif" && fileExtension.ToLower() != "jpeg")
            {
                return Results.BadRequest("Invalid file extension");
            }
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFormFile.CopyToAsync(stream);
            }
            Image newImage = new()
            {
                Title = imageTitle,
                ImagePath = filePath
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
            };
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "imageInfo.json");
            string allJson = await File.ReadAllTextAsync(jsonPath);
            List<Image> allImages = new();
            if (!string.IsNullOrEmpty(allJson))
            {
                allImages = JsonSerializer.Deserialize<List<Image>>(allJson);
            }
            allImages.Add(newImage);
            string allImagesJson = JsonSerializer.Serialize(allImages, options);
            await File.WriteAllTextAsync(jsonPath, allImagesJson);
            return Results.RedirectToRoute("picture", new { id = newImage.Id });
        });

        app.MapGet("/pictures/{id}", async (string id, HttpContext context) =>
        {
            // Retrieve the image information based on the provided ID
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "imageInfo.json");
            string allJson = await File.ReadAllTextAsync(jsonPath);
            List<Image> allImages = JsonSerializer.Deserialize<List<Image>>(allJson) ?? new List<Image>();
            Image? image = allImages.Find(i => i.Id == id);

            if (image is null)
            {
                // Image not found, return a NotFound result
                return Results.NotFound("Image not Found");
            }
            else
            {
                // Image found, display the image and its details
                byte[] imageBytes = await File.ReadAllBytesAsync(image.ImagePath);
                string imageBase64Data = Convert.ToBase64String(imageBytes);

                var html = $@"<!DOCTYPE html>
                <html>
                    <head>
                        <meta charset=""utf-8"" />
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Uploaded Image</title>
                        <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-9ndCyUaIbzAi2FUVXJi0CjmCapSmO7SnpJef0486qhLnuZ2cdeRhO02iuK6FUUVM"" crossorigin=""anonymous"">
                    </head>
                    <body>
                        <div class=""container card shadow p-0 d-flex justify-content-center mt-5"" style=""width: 25rem;"">
                            <img src=""data:image/png;base64,{imageBase64Data}"" alt=""{image.Title}"" class=""card-img-top"" style=""width: 100%;"" >
                            <div class=""card-body"">
                                <h4 class=""card-title"">Title:</h4>
                                <h5 class=""card-title"">{image.Title}</h5>
                            </div>
                            <div class=""card-body"">
                                <a href=""/"" class=""btn btn-primary"">Back to form</a>
                            </div>
                        </div>
                    </body>
                </html>";

                return Results.Content(html, "text/html", System.Text.Encoding.UTF8);
            }
        }).WithName("picture");

        app.Run();
    }
}
