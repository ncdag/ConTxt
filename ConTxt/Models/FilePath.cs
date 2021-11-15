using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebBrowser
{
    public class FilePath
    {
        public string fullPath { get; private set;}
        public string providedRelativePath { get; private set;}
        private FilePath(string relativeOrAbsolutePath){
            providedRelativePath = relativeOrAbsolutePath;
            fullPath = Path.GetFullPath(providedRelativePath);
        }
        public async Task<string> getTextOfResidentFileAsync(){
            try{
                return await File.ReadAllTextAsync(fullPath);
            }
            catch{
                return null;
            }
        }
        public static FilePath createFilePath(List<string> pathWithoutSlashes){
            return createFilePath(pathWithoutSlashes.ToArray());
        }
        public static FilePath createFilePath(string[] pathWithoutSlashes){
            try{
                return new FilePath(
                    Path.Combine(pathWithoutSlashes)
                );
            }
            catch{
                return null;
            }
        }
    }
}