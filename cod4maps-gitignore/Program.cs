using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using CommandLine;

namespace cod4maps_gitignore
{
  class Program
  {
    static void Main(string[] args)
    {
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(RunWithExitCode);
    }
    
    private static void RunWithExitCode(Options options)
    {
      options.WriteOptions();    
      
      //Console.ReadLine();
      
      var checksums = GetOrCreateChecksums(options.ChecksumFilePath, options.ModToolsDirectory);
      
      //Console.ReadLine();
      
      var partialIgnores = GetGitPartialIgnoreEntries(options.GitIgnorePartialFilePath);
      
      var ignores = GetIgnoresForDirectory(
        options.SourcesDirectory, 
        options.SourcesDirectory, 
        checksums.ToDictionary(
          a => a.FilePath, 
          b => b), 
        out bool nothingChanged);
      
      //var ignores = GetGitIgnoreEntries(options.SourcesDirectory, checksums, partialIgnores);
      
      //Console.ReadLine();
      
      WriteGitIgnore(options.GitIgnoreFilePath, options.GitIgnorePartialFilePath, ignores);
    }
    
    private static IEnumerable<string> GetGitPartialIgnoreEntries(string partialFilePath)
    {
      var partialEntries = File.ReadAllText(partialFilePath).Replace("\r", "").Split('\n');
      return partialEntries;
    }
    
    private static IEnumerable<string> GetGitIgnoreEntries(string sourcesDirectory, IEnumerable<FileChecksum> checksums)
    {
      Console.WriteLine($"Comparing working directory with checksums {sourcesDirectory}");
      
      var sourceFiles = Directory.GetFiles(
          sourcesDirectory, "*", SearchOption.AllDirectories)
        .Select(a => GetRelativePath(a, sourcesDirectory))
        .ToHashSet();
      
      var ignores = new List<string>(checksums.Count());
      double i = 0;
      foreach (var checksum in checksums)
      {
        var currentFile = checksum.FilePath;
        if (sourceFiles.Contains(currentFile))
        {         
          var sourceChecksum = CalculateMD5(Path.Combine(sourcesDirectory, currentFile));
          if (sourceChecksum == checksum.Checksum)
          {
            ignores.Add(currentFile);
          }
        }
        
        PrintPercentage(++i / checksums.Count());
      }
      
      Console.WriteLine();   
      return ignores;
    }
    
    private static IEnumerable<string> GetIgnoresForDirectory(string sourcesDirectory, 
      string searchDirectory, 
      Dictionary<string, FileChecksum> checksums, 
      out bool nothingChanged)
    {
      nothingChanged = true;
      
      var dirIgnores = new List<string>();
   
      var sourceDirectories = Directory.GetDirectories(
        searchDirectory, "*", SearchOption.TopDirectoryOnly);
      
      foreach (var currDir in sourceDirectories)
      {
        dirIgnores.AddRange(
          GetIgnoresForDirectory(
            sourcesDirectory, currDir, checksums, out bool currNothingChanged));
        
        nothingChanged &= currNothingChanged;
      }
      
      Console.WriteLine($"Comparing directory {searchDirectory}");
      
      var sourceFiles = Directory.GetFiles(
          searchDirectory, "*", SearchOption.TopDirectoryOnly)
        .Select(a => GetRelativePath(a, sourcesDirectory))
        .ToHashSet();
      
      var fileIgnores = new List<string>(sourceFiles.Count());
      //double i = 0;
      foreach (var sourceRelativePath in sourceFiles)
      {
        if (checksums.TryGetValue(sourceRelativePath, out var checksum))
        {
          var sourceChecksum = CalculateMD5(Path.Combine(sourcesDirectory, sourceRelativePath));
          if (sourceChecksum == checksum.Checksum)
          {
            fileIgnores.Add(sourceRelativePath);
          }
          else
          {
            // file is different
            nothingChanged = false;
          }
        }
        else
        {
          // file was added
          nothingChanged = false;
        }
        
        //PrintPercentage(++i / sourceFiles.Count());
      }
      
      //Console.WriteLine();   
      
      if (nothingChanged)
      {
        return new List<string>() 
        { 
          GetRelativePath(searchDirectory, sourcesDirectory) + "/*"
        };
      }
      else
      {
        return dirIgnores.Union(fileIgnores).ToList();
      }
    }
    
    private static void WriteGitIgnore(string filePath, string partialFilePath, IEnumerable<string> entries)
    {
      Console.WriteLine($"Creating .gitignore file {filePath}");
      
      var partial = File.ReadAllText(partialFilePath);
      var ignores = string.Join("\r\n", entries.Select(a => a.Replace('\\', '/')));
      
      File.WriteAllText(filePath, partial + "\r\n" + ignores);
    }
    
    private static IEnumerable<FileChecksum> GetOrCreateChecksums(string checksumFilePath, string modToolsDirectory)
    {
      if (File.Exists(checksumFilePath))
      {
        Console.WriteLine($"Reading existing checksum file {checksumFilePath}");
        return FileChecksum.ReadFromCSV(checksumFilePath);
      }
      else
      {
        Console.WriteLine($"Generating checksums from {modToolsDirectory}");
        if (!Directory.Exists(modToolsDirectory))
        {
          throw new DirectoryNotFoundException(modToolsDirectory);
        }
        
        var checksums = GenerateChecksums(modToolsDirectory).ToList();
        
        Console.WriteLine($"Creating new checksum file {checksumFilePath}");
        FileChecksum.WriteToCSV(checksumFilePath, checksums);
        
        return checksums;
      }
    }
    
    private static string GetRelativePath(string path, string rootPath)
    {
      return path.Replace(rootPath, "").Substring(1);
    }
    
    private static IEnumerable<FileChecksum> GenerateChecksums(string directory)
    {
      var modToolsFiles = Directory.GetFiles(
          directory, "*", SearchOption.AllDirectories);
      
      var checksums = new List<FileChecksum>(modToolsFiles.Length);
      double i = 0;
      foreach (var file in modToolsFiles)
      {
        checksums.Add(
          new FileChecksum() 
          {
            FilePath = GetRelativePath(file, directory),
            Checksum = CalculateMD5(file)
          });
        
        PrintPercentage(++i / modToolsFiles.Length);
      }
      
      Console.WriteLine();
      return checksums;
    }
    
    private static int _lastPercentage;
    private static void PrintPercentage(double fraction)
    {
      var percentage = (int)(fraction * 100);
      if (_lastPercentage != percentage)
      {
        _lastPercentage = percentage;
        Console.Write($"\r{percentage}%");
      }
    }
    
    static string CalculateMD5(string filename)
    {
      using (var md5 = MD5.Create())
      {
        using (var stream = File.OpenRead(filename))
        {
          var hash = md5.ComputeHash(stream);
          return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
      }
    }
  }
}