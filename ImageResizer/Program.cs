using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
/// Resize all images in the program's directory and subdirectories to the specified size.
/// Skip conversions that would lead to poorer quality.
/// To modify which images to convert, see the <c>ProcessChosenExtensions</c> method.
/// 
/// Denis Labrecque
/// June 1, 2019
/// </summary>
namespace ImageResizer
{
   class Program
   {
      static string m_imageSuffix = "-th";
      static string m_currentFolder;
      static long m_imageQuality = 85L;
      static int m_imageWidth = 420;
      static int m_successCount = 0;
      static int m_errorCount = 0;

      public static void Main()
      {
         m_currentFolder = Directory.GetCurrentDirectory();

         // Print a start message that confirms which directory the program is processing
         PrintStartMessage();

         // Check whether or not to change the presets, and modify them if wanted by the user
         ConfirmPresets();

         // Recurse down through the current directory, processing any images according to settings
         ProcessDirectory(m_currentFolder);

         PrintEndMessage();
      }


      /// <summary>
      /// Confirm that processing is complete, showing how many successes and failures occurred.
      /// Wait for user input to close the program.
      /// </summary>
      private static void PrintEndMessage()
      {
         Console.WriteLine("Successfully saved " + m_successCount + " new images with " + m_errorCount + " errors.");

         string response;
         do
         {
            Console.Write("Run again? (Y, N) ");
            response = Console.ReadLine();
            response = response.ToLower();
         } while(string.IsNullOrWhiteSpace(response));
         Console.WriteLine("\n");

         if(response.Equals("y"))
            Main();
      }


      /// <summary>
      /// Print the current defaults and ask whether to override them.
      /// If the user wants to override, ask for his input.
      /// </summary>
      private static void ConfirmPresets()
      {
         Console.WriteLine("Image width       : " + m_imageWidth + "px");
         Console.WriteLine("Image compression : " + m_imageQuality + "%");
         Console.WriteLine("Thumbnail suffix  : " + m_imageSuffix);

         string response;
         do
         {
            Console.Write("Defaults OK? (Y, N) ");
            response = Console.ReadLine();
            response = response.ToLower();
         } while(string.IsNullOrWhiteSpace(response));

         if(response.Equals("y"))
            return;
         else
            ChangePresets();
      }


      /// <summary>
      /// Ask the user values for all the presets, and change the values.
      /// </summary>
      private static void ChangePresets()
      {
         string response;

         do // Set image width
         {
            Console.Write("Set the desired image width (currently " + m_imageWidth + "px) ");
            response = Console.ReadLine();
         } while(!int.TryParse(response, out m_imageWidth));

         do // Image quality
         {
            Console.Write("Set the image compression (currently " + m_imageQuality + "%) ");
            response = Console.ReadLine();
         } while(!long.TryParse(response, out m_imageQuality));
         Clamp<long>(m_imageQuality, 100L, 0L);

         do // Thumbnail suffix
         {
            Console.Write("Set the thumbnail suffix (currently " + m_imageSuffix + ") ");
            response = Console.ReadLine();
         } while(string.IsNullOrWhiteSpace(response));
         m_imageSuffix = response;

         Console.WriteLine();
      }


      /// <summary>
      /// Confirm the current directory.
      /// </summary>
      static void PrintStartMessage()
      {
         Console.WriteLine("---------------------------------------------------------------------------");
         Console.WriteLine("This program rescales images in the current directory and subdirectories");
         Console.WriteLine("to be less than a specified pixel width. Images smaller than the specified");
         Console.WriteLine("width will not be resized.");
         Console.WriteLine("Denis Labrecque, June 1, 2019");
         Console.WriteLine("Working in " + m_currentFolder);
         Console.WriteLine("---------------------------------------------------------------------------\n");
      }

      
      /// <summary>
      /// Process all files in the directory.
      /// Recurse any directories found in this one, and process any files they contain.
      /// </summary>
      /// <param name="targetDirectory"></param>
      public static void ProcessDirectory(string targetDirectory)
      {
         // Process the list of files found in the directory.
         string[] fileEntries = Directory.GetFiles(targetDirectory);
         foreach(string fileName in fileEntries)
            ProcessChosenExtensions(fileName);

         // Recurse into subdirectories of this directory.
         string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
         foreach(string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory);
      }


      /// <summary>
      /// Only process files of type PNG and JPG.
      /// </summary>
      /// <param name="path">The path to the file (must be valid)</param>
      public static void ProcessChosenExtensions(string path)
      {
         ImageFormat format;

         switch(Path.GetExtension(path).ToLower())
         {
            case ".jpeg":
            case ".jpg":
               format = ImageFormat.Jpeg;
               break;
            case ".png":
               format = ImageFormat.Png;
               break;
            default:
               format = null;
               break;
         }

         if(format != null)
            RescaleImage(path, format);
         // Skip this file because it's not an image to process
         else
         {
            Console.WriteLine(Path.GetFileName(path) + " skipped.\n");
         }
      }


      /// <summary>
      /// Try to re-encode an image to a new size.
      /// Warn of any mishaps.
      /// </summary>
      /// <param name="filePath">The complete image file path</param>
      /// <param name="format">The image's original format</param>
      private static void RescaleImage(string filePath, ImageFormat format)
      {
         try
         {
            // Get a scaled bitmap. The using statement ensures objects are automatically disposed from memory after use.  
            using(Bitmap bitmap = (Bitmap)ScaleImageWidth(new Bitmap(filePath), m_imageWidth))
            {
               // Get the encoder for either PNG or JPG.
               ImageCodecInfo formatEncoder = GetEncoder(format);

               // Create an Encoder object based on the GUID for the Quality parameter category.  
               System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

               // An EncoderParameters object has an array of EncoderParameter objects.
               // In this case, there is only one EncoderParameter object in the array.  
               EncoderParameters encoderParams = new EncoderParameters(1);

               EncoderParameter parameter = new EncoderParameter(encoder, m_imageQuality);
               encoderParams.Param[0] = parameter;

               string savePath = Path.GetDirectoryName(filePath) + @"\" + Path.GetFileNameWithoutExtension(filePath) + m_imageSuffix + ".jpg";

               try
               {
                  bitmap.Save(savePath, formatEncoder, encoderParams);
                  m_successCount++;
                  Console.WriteLine("Thumbnail for " + Path.GetFileName(filePath) + " saved.");
                  Console.WriteLine();
               }
               catch(Exception e)
               {
                  m_errorCount++;
                  Console.WriteLine(e.Message + "\n");
               }
            }
         }
         // The thumbnail would have been larger than the original
         catch(InvalidOperationException e)
         {
            m_errorCount++;
            Console.WriteLine(e.Message + "\n");
         }
         catch(Exception e)
         {
            m_errorCount++;
            Console.WriteLine(e.Message + "\n");
         }
      }


      /// <summary>
      /// Scale an image proportionally with a maximum width and no maximum height.
      /// May throw an <c>InvalidOperationException</c> if the new image is to be rescaled larger.
      /// </summary>
      /// <param name="image">The image to scale</param>
      /// <param name="maxWidth">The width to scale the image to</param>
      /// <returns>The scaled <code>Image</code></returns>
      public static Image ScaleImageWidth(Image image, int maxWidth)
      {
         return ScaleImage(image, maxWidth, image.Height);
      }


      /// <summary>
      /// Scale an image proportionally with a maximum height and no maximum width.
      /// May throw an <c>InvalidOperationException</c> if the new image is to be rescaled larger.
      /// </summary>
      /// <param name="image">The image to scale</param>
      /// <param name="maxHeight">The height to scale the image to</param>
      /// <returns>The scaled <code>Image</code></returns>
      public static Image ScaleImageHeight(Image image, int maxHeight)
      {
         return ScaleImage(image, image.Width, maxHeight);
      }


      /// <summary>
      /// Scale an image proportionally within a maximum width and height.
      /// May throw an <c>InvalidOperationException</c> if the new image is to be rescaled larger.
      /// https://stackoverflow.com/questions/6501797/resize-image-proportionally-with-maxheight-and-maxwidth-constraints/6501997#6501997
      /// </summary>
      /// <returns>The scaled <c>Image</c></returns>
      public static Image ScaleImage(Image image, int width, int height)
      {
         // Don't do anything if the image is already smaller
         if(width >= image.Width || height >= image.Width)
            throw new InvalidOperationException("An image should not be rescaled larger or equal to its original size.");

         var ratioX = (double)width / image.Width;
         var ratioY = (double)height / image.Height;
         var ratio = Math.Min(ratioX, ratioY);

         var newWidth = (int)(image.Width * ratio);
         var newHeight = (int)(image.Height * ratio);

         var newImage = new Bitmap(newWidth, newHeight);

         using(var graphics = Graphics.FromImage(newImage))
            graphics.DrawImage(image, 0, 0, newWidth, newHeight);

         return newImage;
      }


      /// <summary>
      /// Get the proper encoder for an image format.
      /// </summary>
      /// <param name="format">Image format to convert to</param>
      /// <returns>The appropriate <c>ImageCodecInfo</c></returns>
      private static ImageCodecInfo GetEncoder(ImageFormat format)
      {
         ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
         foreach(ImageCodecInfo codec in codecs)
         {
            if(codec.FormatID == format.Guid)
            {
               Console.WriteLine("Found codec " + codec.CodecName);
               return codec;
            }
         }
         return null;
      }

      public static T Clamp<T>(T value, T max, T min) where T : System.IComparable<T>
      {
         T result = value;
         if(value.CompareTo(max) > 0)
            result = max;
         if(value.CompareTo(min) < 0)
            result = min;
         return result;
      }
   }
}