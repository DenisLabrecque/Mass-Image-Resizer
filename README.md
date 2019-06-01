# Mass Image Resizer Utility
I made this simple console app to allow resizing all my Jekyll website images to thumbnails with a custom suffix name in one fell swoop.

Just download `ImageResizer.exe` from the realeases page in the folder you want images resized. Run the program, and it will resize all images in that folder and in subfolders, skipping images that would otherwise be resized larger by the operation.

Because this program was largely a copy-paste operation to create a utility for myself, I make no statements concerning licensing, but here are the sources:

* [Alex Aza's resize with constraints solution on Stack Overflow](https://stackoverflow.com/questions/6501797/resize-image-proportionally-with-maxheight-and-maxwidth-constraints/6501997#6501997)
* [Microsoft documentation on getting files](https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?redirectedfrom=MSDN&view=netframework-4.8)
* [Microsoft documentation on setting JPEG compression level](https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-set-jpeg-compression-level)