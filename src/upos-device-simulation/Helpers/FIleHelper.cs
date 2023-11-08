using System.IO;
using upos_device_simulation.Interfaces;

namespace upos_device_simulation.Helpers
{
    public class FileHelper:IFileHelper
    {
        public  void CreateLogFile(string filePath)
        {
            if (!File.Exists(filePath))
                File.Create(filePath);
        }
    }
}
