﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoUpgrade.VersionUpdaters;
using McMaster.Extensions.CommandLineUtils;
using System.IO.Abstractions;

namespace AutoUpgrade
{
    public class FileUpdater
    {
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;

        public FileUpdater(IConsole console) : this(null, console)
        {}

        public FileUpdater(IFileSystem fileSystem, IConsole console)	
        {	
            _fileSystem = fileSystem ?? new FileSystem();
            _console = console;
        }

        public void UpdateProjectFiles(IEnumerable<FileInfo> projectFiles, IDotNetVersionUpdater dotNetVersionUpdater)
        {
            UpdateFiles(projectFiles, dotNetVersionUpdater.UpdateProjectFileContents, "No Project files found!");
        }

        public void UpdateDockerFiles(IEnumerable<FileInfo> dockerFiles, IDotNetVersionUpdater dotNetVersionUpdater)
        {
            UpdateFiles(dockerFiles, dotNetVersionUpdater.UpdateDockerFileContent, "No Docker files found!");
        }

        public void UpdateEnvironmentFiles(IEnumerable<FileInfo> envFiles, Version2Point1Updater dotNetVersionUpdater)
        {
            UpdateFiles(envFiles, dotNetVersionUpdater.UpdateEnvFileContent, "No .env files found!");
        }

        private void UpdateFiles(IEnumerable<FileInfo> filesToUpdate, Func<string, string> updateMethod, string noFilesFoundMessage)
        {
            if (filesToUpdate == null || !filesToUpdate.Any())
            {
                _console.WriteLine(noFilesFoundMessage);
                return;
            }

            foreach (var fileInfo in filesToUpdate)
            {
                _console.WriteLine($"Updating {fileInfo.FullName}");
                try
                {
                    string content;
                    using (var streamReader = _fileSystem.File.OpenText(fileInfo.FullName))
                    {
                        content = streamReader.ReadToEnd();
                    }

                    var updatedContent = updateMethod(content);

                    File.WriteAllText(fileInfo.FullName, updatedContent);
                }
                catch (Exception e)
                {
                    _console.WriteLine($"Updating {fileInfo.FullName} failed: {e.Message}");
                }
            }
        }
    }
}