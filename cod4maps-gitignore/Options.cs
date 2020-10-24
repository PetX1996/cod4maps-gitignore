using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

namespace cod4maps_gitignore
{
  public class Options
  { 
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
    
    private string _gitIgnorePartialFilePath;
    [Option('p', "gitignorepartial", Required = false, HelpText = "File path to partial gitignore which will be added.")]
    public string GitIgnorePartialFilePath 
    { 
      get => _gitIgnorePartialFilePath;
      set 
      { 
        if (!File.Exists(value))
        {
          throw new FileNotFoundException(value);
        }
        _gitIgnorePartialFilePath = value;
      }
    }
    
    private string _gitIgnoreFilePath;
    [Option('g', "gitignore", Required = false, HelpText = "File path to generated gitignore.")]
    public string GitIgnoreFilePath 
    { 
      get => _gitIgnoreFilePath ?? Path.Combine(SourcesDirectory, ".gitignore");
      set 
      { 
        if (!File.Exists(value))
        {
          throw new FileNotFoundException(value);
        }
        _gitIgnoreFilePath = value;
      }
    }
    
    private string _modToolsDirectory;
    [Option('m', "modtools", Required = false, HelpText = "Directory path to clean installation of cod4+modtools.")]
    public string ModToolsDirectory
    { 
      get => _modToolsDirectory;
      set 
      { 
        if (!Directory.Exists(value))
        {
          throw new FileNotFoundException(value);
        }
        _modToolsDirectory = value;
      } 
    }
    
    private string _checksumFilePath;
    [Option('c', "checksum", Required = true, HelpText = "File path to csv file containing checksums of all original modtools files.")]
    public string ChecksumFilePath 
    { 
      get => _checksumFilePath;
      set 
      { 
        //if (!File.Exists(value))
        //{
        //  throw new FileNotFoundException(value);
        //}
        _checksumFilePath = value;
      } 
    }
    
    private string _sourcesDirectory;
    [Option('s', "sources", Required = true, HelpText = "Directory path to your sources working directory.")]
    public string SourcesDirectory
    { 
      get => _sourcesDirectory;
      set 
      { 
        if (!Directory.Exists(value))
        {
          throw new FileNotFoundException(value);
        }
        _sourcesDirectory = value;
      } 
    }
    
    public void WriteOptions()
    {
      Console.WriteLine($"Current Arguments:");
      Console.WriteLine($"-v {Verbose}");
      Console.WriteLine($"-m {ModToolsDirectory}");
      Console.WriteLine($"-c {ChecksumFilePath}");
      Console.WriteLine($"-s {SourcesDirectory}");
      Console.WriteLine($"-g {GitIgnoreFilePath}"); 
      Console.WriteLine($"-g {GitIgnorePartialFilePath}"); 
    }
  }
}