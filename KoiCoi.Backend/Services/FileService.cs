using KoiCoi.Backend.Controllers.FileControl;
using KoiCoi.Operational.Encrypt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Operational.Extensions;


public class FileService
{
    //private static IConfigurationBuilder appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
    //private static IConfigurationRoot Configuration = appsettingbuilder.Build();
    private static readonly IConfiguration _configuration = ImageFileServiceController.StaticConfiguration!;
    private static readonly string baseDirectory = _configuration.GetSection("appSettings:UploadPath").Value;
    private static readonly string[] allowext = _configuration.GetSection("appSettings:AllowExtension").Get<string[]>();
    private static readonly string[] allowfunction = _configuration.GetSection("appSettings:AllowFunction").Get<string[]>();


    public static Boolean MoveTempFile(string functionname, string filename, string tempfilename)
    {
        string tempfolderPath = baseDirectory + _configuration.GetSection("appSettings:UploadTempPath").Value;
        tempfilename = Encryption.DecryptFileName(tempfilename);
        if (!allowfunction.Contains(functionname))
        {
            throw new Exception("Function Name Not Allow : " + functionname);
        }
        string ext = GetFileExtension(tempfilename);
        if (!allowext.Contains(ext))
            throw new Exception("Invalid File Extension: " + tempfilename);

        string fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value;
        // tempfilename = Encryption.DecryptFileName(tempfilename);
        string filefullPath = fullPath + filename + '.' + ext;
        string tempfullPath = tempfolderPath + tempfilename;

        if (filefullPath.Contains(".."))
        {  //if found .. in the file name or path
            Log.Error("Invalid path " + fullPath);
            throw new Exception("Invalid file path");
        }
        if (tempfullPath.Contains(".."))
        { //if found .. in the file name or path
            Log.Error("Invalid path " + tempfullPath);
            throw new Exception("Invalid temp path");
        }
        if (!Directory.Exists(fullPath))
        {
            Log.Error("Folder path not found" + fullPath);
            throw new Exception("Folder path not found");
        }

        File.Move(tempfullPath, filefullPath);

        return true;
    }

    public static Boolean MoveTempFileDir(string functionname, string dirname, string tempdirname)
    {

        try
        {
            string tempfolderPath = baseDirectory + _configuration.GetSection("appSettings:UploadTempPath").Value;

            if (!allowfunction.Contains(functionname))
            {
                throw new Exception("Function Name Not Allow : " + functionname);
            }

            string fullPath = "";
            fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value;
            tempdirname = Encryption.DecryptFileName(tempdirname);
            tempfolderPath += tempdirname;
            fullPath += dirname;

            if (fullPath.Contains(".."))
            { //if found .. in the file name or path
                Log.Error("Invalid path " + fullPath);
                throw new Exception("Invalid folder path");
            }

            if (tempfolderPath.Contains(".."))
            { //if found .. in the file name or path
                Log.Error("Invalid path " + tempfolderPath);
                throw new Exception("Invalid temp folder path");
            }

            if (!Directory.Exists(tempfolderPath))
            {
                Log.Error("Temp folder not found " + tempfolderPath);
                throw new Exception("Temp folder not found");
            }

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            string[] files = System.IO.Directory.GetFiles(tempfolderPath);
            string fileName;
            string destFile;

            // Copy the files and overwrite destination files if they already exist.
            foreach (string s in files)
            {
                // Use static Path methods to extract only the file name from the path.
                string ext = Path.GetExtension(s).ToLower().TrimStart('.');
                if (!allowext.Contains(ext))
                    throw new Exception("Invalid File Extension: " + s);

                fileName = Path.GetFileName(s);
                destFile = fullPath + System.IO.Path.DirectorySeparatorChar + fileName;
                System.IO.File.Move(s, destFile, true);
            }
            Directory.Delete(tempfolderPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);  //never return or output file path exception to users, just output to log file
            return false;
        }
    }

    public static Boolean DeleteFileNameOnly(string functionname, string filename)
    {
        try
        {
            if (!allowfunction.Contains(functionname))
            {
                throw new Exception("Function Name Not Allow : " + functionname);
            }

            string fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value;

            if (fullPath.Contains("..") || filename.Contains(".."))
            { //if found .. in the file name or path
                Log.Error("Invalid path " + fullPath + "," + filename);
                throw new Exception("Invalid path");

            }
            string? existingFile = Directory.EnumerateFiles(
                fullPath,
                filename + ".*").FirstOrDefault();

            if (!string.IsNullOrEmpty(existingFile))
            {
                string ext = Path.GetExtension(existingFile).ToLower().TrimStart('.');
                if (!allowext.Contains(ext))
                    throw new Exception("Invalid File Extension: " + ext);

                System.IO.File.Delete(existingFile);
                return true;
            }
            else
            {
                Log.Error("File not found under: " + fullPath + "/" + filename);
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "FileService DeleteFileNameOnly Fail");   //never return or output file path exception to users, just output to log file
            return false;
        }
    }

    public static Boolean DeleteDir(string functionname, string dirname)
    {
        try
        {
            string fullPath = "";

            if (!allowfunction.Contains(functionname))
            {
                throw new Exception("Function Name Not Allow : " + functionname);
            }

            fullPath = baseDirectory + _configuration.GetSection("appSettings:" + functionname).Value + System.IO.Path.DirectorySeparatorChar + dirname;

            if (fullPath.Contains(".."))
            {  //if found .. in the file name or path
                Log.Error("Invalid path: " + fullPath);
                throw new Exception("Invalid path: " + dirname);
            }
            //delete all files (with allow extension) under the folder
            IEnumerable<string> existingFile = Directory.EnumerateFiles(fullPath, "*.*");
            foreach (string currentFile in existingFile)
            {
                string ext = Path.GetExtension(currentFile).ToLower().TrimStart('.');
                if (!allowext.Contains(ext))
                    throw new Exception("Invalid File Extension: " + currentFile);
                else
                    File.Delete(currentFile);
            }

            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, false); //delete without recursive, if some file left, it will trigger error. 
            }
            else
            {
                throw new Exception("Folder Path not found: " + fullPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "FileService DeleteDir Fail");   //never return or output file path exception to users, just output to log file
            return false;
        }

    }

    public static string GetFileExtension(string FileName)
    {
        int lstindex = FileName.LastIndexOf(".");
        string ext = FileName[(lstindex + 1)..];
        //string ext = FileName.Substring(lstindex + 1, FileName.Length - (lstindex + 1));
        return ext.ToLower().TrimStart('.');
    }

}

