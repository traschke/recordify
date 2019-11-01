using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recordify
{
    class Utils
    {
        public static string getFilenameWithCounterPlusExtensionChange(string fullpath, string extension)
        {
            fullpath = Path.ChangeExtension(fullpath, extension);
            
            string fileNameOnly = Path.GetFileNameWithoutExtension(fullpath);
            string ext = Path.GetExtension(fullpath);
            string path = Path.GetDirectoryName(fullpath);
            string newFullPath = fullpath;
            int counter = 1;
                
            while(File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}-{1}", fileNameOnly, counter++);
                newFullPath = Path.Combine(path, tempFileName + "." + extension);
            }
            
            return newFullPath;
        }

        public static string cleanSpecialCharacters(string value)
        {
            value = value.Replace("/", "");
            value = value.Replace(":", "");
            value = value.Replace("*", "");
            value = value.Replace("?", "");
            value = value.Replace("\"", "");
            value = value.Replace("<", "");
            value = value.Replace(">", "");
            value = value.Replace("|", "");
            return value;
        }
    }
}
