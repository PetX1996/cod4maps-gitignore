using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace cod4maps_gitignore
{
  public class FileChecksum
  {
    public string FilePath { get; set; }
    public string Checksum { get; set; }
    
    public static IEnumerable<FileChecksum> ReadFromCSV(string filePath)
    {
      using (var reader = new StreamReader(filePath))
      using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
      {    
        return csv.GetRecords<FileChecksum>().ToList();
      }
    }
    
    public static void WriteToCSV(string filePath, IEnumerable<FileChecksum> records)
    {
      using (var writer = new StreamWriter(filePath))
      using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
      {    
        csv.WriteRecords(records);
      }
    }
  }
}