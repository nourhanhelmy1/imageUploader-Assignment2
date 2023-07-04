using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async (HttpContext context) =>
{
    await context.Response.WriteAsync(@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""utf-8"" />
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>My Image Uploader</title>
            <style>
                body {
                    background-color: #FCE4EC;
                    color: #333333;
                    font-family: Arial, sans-serif;
                    margin: 0;
                    padding: 20px;
                }
        
                h1 {
                    color: #FF4081;
                    text-align: center;
                }
        
                form {
                    background-color: #FFFFFF;
                    border-radius: 5px;
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                    padding: 20px;
                    max-width: 400px;
                    margin: 0 auto;
                }
        
                label {
                    display: block;
                    font-weight: bold;
                    margin-bottom: 10px;
                }
        
                input[type=""text""],
                input[type=""file""] {
                    width: 100%;
                    padding: 8px;
                    margin-bottom: 15px;
                    border: 1px solid #CCCCCC;
                    border-radius: 4px;
                }
        
                input[type=""submit""] {
                    background-color: #FF4081;
                    border: none;
                    color: #FFFFFF;
                    cursor: pointer;
                    padding: 10px 20px;
                    font-size: 16px;
                    border-radius: 4px;
                }
        
                input[type=""submit""]:hover {
                    background-color: #E91E63;
                }
            </style>
        </head>
        <body>
            <h1>Image Uploader</h1>
            <form action=""/"" method=""POST"" enctype=""multipart/form-data"">
                <label for=""name"">Name:</label>
                <input type=""text"" id=""name"" name=""name"" required><br><br>
                <label for=""image"">Attach Image:  (JPEG, PNG, GIF)</label>
                <input type=""file"" id=""image"" name=""image"" accept=""image/jpeg, image/png, image/gif"" required><br><br>
                <input type=""submit"" value=""Upload"">
            </form>
        </body>
        </html>");
});

app.MapPost("/", async (HttpContext context) =>
{
    IFormCollection imageForm = await context.Request.ReadFormAsync();
    var imageFile = imageForm.Files[0];
    string imageId = Guid.NewGuid().ToString();
    string imageName = imageForm["name"];
    string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

    if (imageForm.Files.Count == 0)
    {
        context.Response.StatusCode = 400; //Bad Request
        await context.Response.WriteAsync("No image uploaded!");
        return;
    }

    if (fileExtension != ".png" && fileExtension != ".gif" && fileExtension != ".jpeg")
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("File extension is not allowed. Accepts only JPEG, PNG and GIF!");
        return;
    }

    if (string.IsNullOrEmpty(imageName))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Please enter the image title!");
        return;
    }

    var imagePath = Path.Combine("images", $"{imageId}{fileExtension}");
    using (var stream = new FileStream(imagePath, FileMode.Create))
    {
        await imageFile.CopyToAsync(stream);
    }

    var base64Image = Convert.ToBase64String(await File.ReadAllBytesAsync(imagePath));
    var Image = new Image
    {
        Id = imageId,
        Name = imageName,
        Path = base64Image
    };

    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true,
    };

    File.WriteAllText("imageInfo.json", JsonSerializer.Serialize(Image, jsonOptions));

    context.Response.Redirect($"/pictures/{imageId}");
});

app.MapGet("/pictures/{id}", async (HttpContext context) =>
{
    Image? newImage = JsonSerializer.Deserialize<Image>(await File.ReadAllTextAsync("imageInfo.json"));

    await context.Response.WriteAsync($@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""utf-8"" />
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Uploaded Image</title>
            <style>
                body {{
                    background-color: #FCE4EC;
                    color: #333333;
                    font-family: Arial, sans-serif;
                    margin: 0;
                    padding: 20px;
                }}
        
                .container {{
                    background-color: #FFFFFF;
                    border-radius: 5px;
                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                    padding: 20px;
                    max-width: 400px;
                    margin: 0 auto;
                }}
        
                .card-img-top {{
                    width: 100%;
                }}
        
                .card-title1 {{
                    color: #FF4081;
                    margin-bottom: 10px;
                }}

                .card-title2 {{
                    color: #000000;
                    margin-bottom: 10px;
                }}
        
                .btn-primary {{
                    background-color: #FF4081;
                    border: none;
                    color: #FFFFFF;
                    cursor: pointer;
                    padding: 10px 20px;
                    font-size: 16px;
                    border-radius: 4px;
                    text-decoration: none;
                    display: inline-block;
                }}
        
                .btn-primary:hover {{
                    background-color: #E91E63;
                }}
            </style>
        </head>
        <body>
            <div class=""container"">
                <img src=""data:image/png;base64,{newImage.Path}"" alt=""{newImage.Name}"" class=""card-img-top"">
                <div class=""card-body"">
                    <h4 class=""card-title1"">Name:</h4>
                    <h5 class=""card-title2"" style=""margin-top: -10px;"">{newImage.Name}</h5>
                </div>
                <div class=""card-body"">
                    <a href=""/"" class=""btn btn-primary"">Back to form</a>
                </div>
            </div>
        </body>
        </html>");
});

app.Run();

public class Image
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
}
