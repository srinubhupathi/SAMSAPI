using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace SAMSAPI.Areas.Common
{
    public class CommonMethod
    {
        public string GetHashedPassword(string password)
        {
            string _salt = ConfigurationManager.AppSettings["Salt"].ToString();
            string _alg = ConfigurationManager.AppSettings["Alg"].ToString();
            string key = string.Join(":", new string[] { password, _salt });
            using (HMAC hmac = HMACSHA256.Create(_alg))
            {
                // Hash the key.
                hmac.Key = Encoding.UTF8.GetBytes(_salt);
                hmac.ComputeHash(Encoding.UTF8.GetBytes(key));
                return Convert.ToBase64String(hmac.Hash);
            }
        }
        public string ConvertToCamelCase(string value)
        {
            char[] array = value.ToCharArray();
            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }
        public int ConvertToInt(string value)
        {
            return (String.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value));
        }

        /// <summary>
        /// Checks for provided file is image or not
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsValidImageFile(string fileName)
        {
            string mimeType = GetMimeType(fileName);
            if (mimeType == "image/png" || mimeType == "image/jpeg" || mimeType == "image/pjpeg" || mimeType == "image/gif")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the mime type of a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
        /// <summary>
        /// This function create the thumbnail of a image
        /// </summary>
        /// <param name="fileName">Parameter with relative path</param>
        /// <param name="thumbnailImagePath">Pass the thumnail if already defined otherwise String.Empty</param>
        /// <returns></returns>
        //public string CreateImageThumbnail(string fileName, string thumbnailImagePath, int id)
        //{
        //    DateTime currentDateTime = DateTime.Now;
        //    string unixTime = Convert.ToString(((DateTimeOffset)currentDateTime).ToUnixTimeSeconds());
        //    if (String.IsNullOrEmpty(thumbnailImagePath))
        //    {
        //        thumbnailImagePath = ConstantVariables.PATH_THUMBNAIL + "/" + id + "_" + unixTime + "_image_thumbnail.Jpeg";
        //    }

        //    string originalImagePath = fileName;

        //    if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("~/" + thumbnailImagePath)))
        //    {
        //        System.Drawing.Image imThumbnailImage;
        //        System.Drawing.Image OriginalImage = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("~/" + originalImagePath));

        //        double originalWidth = OriginalImage.Width;
        //        double originalHeight = OriginalImage.Height;

        //        double ratioX = (double)ConstantVariables.THUMBNAIL_WIDTH / (double)originalWidth;
        //        double ratioY = (double)ConstantVariables.THUMBNAIL_HEIGHT / (double)originalHeight;

        //        double ratio = ratioX < ratioY ? ratioX : ratioY; // use whichever multiplier is smaller

        //        // now we can get the new height and width
        //        int newHeight = Convert.ToInt32(originalHeight * ratio);
        //        int newWidth = Convert.ToInt32(originalWidth * ratio);

        //        imThumbnailImage = OriginalImage.GetThumbnailImage(newWidth, newHeight,
        //                     new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);
        //        imThumbnailImage.Save(HttpContext.Current.Server.MapPath("~/" + thumbnailImagePath), System.Drawing.Imaging.ImageFormat.Jpeg);



        //        imThumbnailImage.Dispose();
        //        OriginalImage.Dispose();
        //        imThumbnailImage = null;
        //        OriginalImage = null;
        //    }
        //    else
        //    {
        //        //logger.Info("Create Image Thumbnail - Thumbnail already exist");
        //    }


        //    return thumbnailImagePath;
        //}
        public bool ThumbnailCallback()
        {
            return false;
        }
        /// <summary>
        /// This function upload the media
        /// That media may be image, video or audio
        /// </summary>
        /// <param name="filePath">Filepath is required where to upload the media</param>
        /// <returns></returns>
        //public string UploadMedia(string filePath, int id)
        //{
        //    string mediaPath = String.Empty;
        //    var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
        //    string fileName = String.Empty;
        //    if (file != null && file.ContentLength > 0)
        //    {
        //        DateTime currentDateTime = DateTime.Now;
        //        string unixTime = Convert.ToString(((DateTimeOffset)currentDateTime).ToUnixTimeSeconds());
        //        fileName = id + "_" + unixTime + "_" + Path.GetFileName(file.FileName);

        //        if (!File.Exists(HttpContext.Current.Server.MapPath("~/" + filePath)))
        //        {
        //            System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/" + filePath));
        //        }


        //        var path = Path.Combine(
        //            HttpContext.Current.Server.MapPath("~/" + filePath),
        //            fileName
        //        );

        //        file.SaveAs(path);
        //        mediaPath = filePath + "/" + fileName;

        //    }
        //    return mediaPath;
        //}

        /// <summary>
        /// This function upload the media
        /// That media may be image, video or audio
        /// </summary>
        /// <param name="filePath">Filepath is required where to upload the media</param>
        /// <returns></returns>
        //public string UploadMedia(string filePath, string imgBase64, string imgName)
        //{
        //    string mediaPath = String.Empty;
        //    string fileName = String.Empty;
        //    try
        //    {
        //        byte[] bytes = Convert.FromBase64String(imgBase64);
        //        DateTime currentDateTime = DateTime.Now;
        //        string unixTime = Convert.ToString(((DateTimeOffset)currentDateTime).ToUnixTimeSeconds());
        //        fileName = unixTime + "_" + imgName;
        //        var path = Path.Combine(
        //             HttpContext.Current.Server.MapPath("~/" + filePath),
        //             fileName
        //         );

        //        if (!File.Exists(HttpContext.Current.Server.MapPath("~/" + filePath)))
        //        {
        //            System.IO.Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/" + filePath));
        //        }
        //        using (Image image = Image.FromStream(new MemoryStream(bytes)))
        //        {
        //            image.Save(path);
        //        }
        //        mediaPath = fileName;
        //    }
        //    catch (Exception ex)
        //    { }
        //    return mediaPath;
        //}
        /// <summary>
        /// This function delete the media file from the server
        /// </summary>
        /// <param name="fileWithPath"></param>
        /// <returns></returns>
        public bool DeleteMedia(string fileWithPath)
        {
            if (!String.IsNullOrEmpty(fileWithPath))
            {
                if (File.Exists(HttpContext.Current.Server.MapPath("~/" + fileWithPath)))
                {
                    File.Delete(HttpContext.Current.Server.MapPath("~/" + fileWithPath));
                }
            }
            return true;
        }

        /// <summary>
        /// This function delete the media files in a directory from the server
        /// </summary>
        /// <param name="fileWithPath"></param>
        /// <returns></returns>
        public bool DeleteMediaDirectory(string fileDirPath)
        {
            if (!String.IsNullOrEmpty(fileDirPath))
            {
                if (Directory.Exists(HttpContext.Current.Server.MapPath("~/" + fileDirPath)))
                {
                    try
                    {
                        Directory.Delete(HttpContext.Current.Server.MapPath("~/" + fileDirPath), true);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                }
            }
            return true;
        }

        /// <summary>
        /// This function delete the media files in a directory from the server
        /// </summary>
        /// <param name="fileWithPath"></param>
        /// <returns></returns>
        public bool DeleteMedia(string fileDirPath, List<string> lstImages)
        {
            if (!String.IsNullOrEmpty(fileDirPath))
            {
                if (Directory.Exists(HttpContext.Current.Server.MapPath("~/" + fileDirPath)))
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(HttpContext.Current.Server.MapPath("~/" + fileDirPath)))
                        {
                            string selImage = null;

                            selImage = lstImages.Where(x => x == file.Split('\\')[file.Split('\\').Length - 1]).FirstOrDefault<string>();
                            if (selImage == null)
                            {
                                File.Delete(file);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }

                }
            }
            return true;
        }


        static string base64String = null;
        public string ImageToBase64(string filePath)
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath("~/" + filePath);
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(path))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";//"Error_" + ex.Message;
            }
        }
        public System.Drawing.Image Base64ToImage()
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }

    }
}