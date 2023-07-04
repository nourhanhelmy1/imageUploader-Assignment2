# imageUploader-Assignment2
## Image Uploader

This is a simple image uploader web application built using C# and .NET. It allows users to upload images with a name and view the uploaded images.

### Getting Started

To run the application, make sure you have the .NET SDK installed. Then, follow these steps:

1. Clone the repository or create a new project.
2. Open the `Program.cs` file in your preferred C# IDE.
3. Build and run the application.

### Usage

1. Access the application through your web browser at `http://localhost:5000`.
2. You will see a form with fields to enter the name and upload an image.
3. Fill in the name field and choose an image file (JPEG, PNG, or GIF) to upload.
4. Click the "Upload" button.
5. If the upload is successful, you will be redirected to a page displaying the uploaded image and its name.
6. To upload another image, click the "Back to form" button.

### Dependencies

The application relies on the following dependencies:

- `Microsoft.AspNetCore` (version 5.0.0 or later)
- `System.Text.Json`

These dependencies are automatically resolved and restored during the build process using the .NET package manager.

### File Structure

- `Program.cs`: Contains the main application logic and routing.
- `imageInfo.json`: JSON file to store the image information.
- `images/`: Folder to store the uploaded image files.

### Limitations

- Only JPEG, PNG, and GIF file types are allowed for uploading.
- The application currently supports uploading and viewing one image at a time. If a new image is uploaded, it overwrites the previous image's information.

Feel free to modify the code and enhance the application to meet your specific requirements.



