namespace LogReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using LogReader.Models;

public static class LogsParser {

    /// <summary>
    /// Reads and retrieves log entries from all files in the GrafanaLogs folder specified by the application parameters.
    /// 
    /// This method performs the following tasks:
    /// 1. Initializes a message to indicate the start of the log reading process.
    /// 2. Validates the existence of the GrafanaLogs directory.
    /// 3. Iterates through each log file in the specified directory, reading its content line by line.
    /// 4. Counts and accumulates the total number of lines read across all log files.
    /// 5. Catches and logs any exceptions encountered while attempting to read individual log files.
    /// 6. Stops a timer to measure the total time taken for the log reading operation.
    /// 7. Outputs a summary message indicating the completion of the log reading process, along with the total number of lines read and the time taken.
    /// 
    /// The method returns a string message summarizing the total number of lines read.
    /// </summary>
    /// <returns>A string summary indicating the number of lines read.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown if the GrafanaLogs directory does not exist.</exception>
    /// <exception cref="Exception">Thrown if there is an error while reading a file.</exception>
    public static List<string> ReadLogs() {

        Console.WriteLine("\nStarting to read logs...");

        string grafanaLogsPath = ConfigReader.appParameters.LogFolder;
        List<string> allLines = [];

        if (string.IsNullOrEmpty(grafanaLogsPath) || !Directory.Exists(grafanaLogsPath))
            throw new DirectoryNotFoundException("The specified GrafanaLogs directory does not exist.");

        int lineCount = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try {
            string[] logFiles = Directory.GetFiles(grafanaLogsPath, "*.*"); // Get all files
            foreach (string file in logFiles) {
                try {
                    Console.WriteLine($" > {file}");
                    string[] lines = File.ReadAllLines(file);
                    lineCount += lines.Length;
                    allLines.AddRange(lines);
                }
                catch (Exception ex) {
                    // If an error occurs while reading, throw the exception
                    throw new Exception($"Error reading file {file}: {ex.Message}");
                }
            }
        }
        finally {
            stopwatch.Stop();
            Console.WriteLine($"Finished reading logs. Number of lines read: {lineCount}. Time taken: {stopwatch.Elapsed.TotalSeconds} seconds.");
        }

        return allLines;
    }

    /// <summary>
    /// Filters a list of log entries to extract only those that contain specific criteria.
    /// 
    /// This method specifically retrieves lines that contain "uname=" followed immediately by a letter or digit (word character).
    /// It omits any lines where "uname=" is followed by non-alphanumeric characters.
    /// 
    /// The filtering process involves the following steps:
    /// 1. Initializes a new list to hold the filtered log entries.
    /// 2. Iterates through each line in the provided <paramref name="allLines"/> list.
    /// 3. Uses a regular expression to check if the line matches the criteria of containing "uname=" followed by a valid character.
    /// 4. If a line meets the criteria, it is added to the filtered list.
    /// 5. Returns the filtered list of log entries that match the specified criteria.
    /// 
    /// This method allows users to easily isolate relevant log entries for further analysis or processing.
    /// </summary>
    /// <param name="allLines">A list of log entries as strings to be filtered.</param>
    /// <returns>A list of strings that contains filtered log entries where "uname=" is followed by a valid word character.</returns>
    public static List<string> FilterLogs(List<string> allLines) {
        List<string> filteredLines = [];

        // Define the regex pattern for "uname=" followed by a letter or digit
        string pattern = @"uname=\w"; // \w matches any letter, digit or underscore

        foreach (string line in allLines) {
            if (Regex.IsMatch(line, pattern)) {
                filteredLines.Add(line);
            }
        }

        return filteredLines;
    }

    /// <summary>
    /// Parses a list of log entries from the provided lines and creates a collection of <see cref="LogEntry"/> objects.
    /// 
    /// This method performs the following operations:
    /// 1. Initializes a list to hold the parsed log entries.
    /// 2. Iterates through each log line in the provided <paramref name="lines"/> list.
    /// 3. For each line, extracts various pieces of information such as timestamp, user ID, organization ID,
    ///    username, log level, message, request method, request path, status, remote address, processing time,
    ///    size of the response, referer, plugin ID, and type.
    /// 4. Handles any parsing errors encountered during the extraction process and records them in the
    ///    <see cref="LogEntry.ParsingErrors"/> property to ensure that all lines are processed regardless of errors.
    /// 5. Returns the list of parsed log entries with all extracted information, allowing further analysis or processing.
    /// 
    /// This method is essential for converting raw log data into structured data for easier management and analysis.
    /// </summary>
    /// <param name="lines">A list of strings representing raw log entries to be parsed.</param>
    /// <returns>A list of <see cref="LogEntry"/> objects populated with the parsed data from the log entries.</returns>
    /// <exception cref="FormatException">Thrown if the log entry format is unexpected and cannot be parsed correctly.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the provided list of log lines is null.</exception>
    public static List<LogEntry> ParseLogs(List<string> lines) {
        List<LogEntry> logEntries = [];

        foreach (string line in lines) {
            LogEntry entry = new() {
                ParsingErrors = string.Empty // Initialize parsing errors
            };

            try {
                // Parse timestamp
                if (TryParseTimestamp(line, out DateTime timestamp)) {
                    entry.Timestamp = timestamp;
                }
                else {
                    entry.ParsingErrors += "Failed to parse Timestamp.\n";
                }

                // Parse userId
                if (TryParseValue(line, "userId", out int userId)) {
                    entry.UserId = userId;
                }
                else {
                    entry.ParsingErrors += "Failed to parse UserId.\n";
                }

                // Parse orgId
                if (TryParseValue(line, "orgId", out int orgId)) {
                    entry.OrgId = orgId;
                }
                else {
                    entry.ParsingErrors += "Failed to parse OrgId.\n";
                }

                // Parse username
                if (TryParseStringValue(line, "uname", out string username)) {
                    entry.Username = username;
                }
                else {
                    entry.ParsingErrors += "Failed to parse Username.\n";
                }

                // Parse logLevel
                if (TryParseStringValue(line, "level", out string logLevel)) {
                    entry.LogLevel = logLevel;
                }
                else {
                    entry.ParsingErrors += "Failed to parse LogLevel.\n";
                }

                // Parse message
                if (TryParseMessage(line, out string message)) {
                    entry.Message = message;
                }
                else {
                    entry.ParsingErrors += "Failed to parse Message.\n";
                }

                // Parse requestMethod
                if (TryParseStringValue(line, "method", out string requestMethod)) {
                    entry.RequestMethod = requestMethod;
                }
                else {
                    entry.ParsingErrors += "Failed to parse RequestMethod.\n";
                }

                // Parse requestPath
                if (TryParseStringValue(line, "path", out string requestPath)) {
                    entry.RequestPath = requestPath;
                }
                else {
                    entry.ParsingErrors += "Failed to parse RequestPath.\n";
                }

                // Parse status
                if (TryParseValue(line, "status", out int? status)) {
                    entry.Status = status;
                }
                else {
                    entry.ParsingErrors += "Failed to parse Status.\n";
                }

                // Parse remoteAddress
                if (TryParseStringValue(line, "remote_addr", out string remoteAddress)) {
                    entry.RemoteAddress = remoteAddress;
                }
                else {
                    entry.ParsingErrors += "Failed to parse RemoteAddress.\n";
                }

                // Parse timeMs
                if (TryParseValue(line, "time_ms", out long? timeMs)) {
                    entry.TimeMs = timeMs;
                }
                else {
                    entry.ParsingErrors += "Failed to parse TimeMs.\n";
                }

                // Parse size
                if (TryParseValue(line, "size", out int? size)) {
                    entry.Size = size;
                }
                else {
                    entry.ParsingErrors += "Failed to parse Size.\n";
                }

                // Parse referer
                if (TryParseStringValue(line, "referer", out string referer)) {
                    entry.Referer = referer;
                }
                else {
                    entry.ParsingErrors += "Failed to parse Referer.\n";
                }

            }
            catch (Exception ex) {
                entry.ParsingErrors += $"General parsing error: {ex.Message}\n";
            }

            logEntries.Add(entry);
        }

        return logEntries;
    }

    private static bool TryParseTimestamp(string line, out DateTime timestamp) {
        Match timestampMatch = Regex.Match(line, @"t=(?<timestampValue>[^ ]+)");
        if (timestampMatch.Success) {
            return DateTime.TryParse(timestampMatch.Groups["timestampValue"].Value, out timestamp);
        }
        timestamp = default; // Ensure timestamp is set to default
        return false;
    }

    private static bool TryParseValue(string line, string key, out int value) {
        Match match = Regex.Match(line, $"{key}=(?<value>-?\\d+)");
        if (match.Success) {
            return int.TryParse(match.Groups["value"].Value, out value);
        }
        value = default; // Ensure value is set to default
        return false;
    }

    private static bool TryParseValue(string line, string key, out long? value) {
        Match match = Regex.Match(line, $"{key}=(?<value>-?\\d+)");
        if (match.Success) {
            bool parsed = long.TryParse(match.Groups["value"].Value, out long temp);
            value = parsed ? (long?)temp : null;
            return parsed;
        }
        value = null; // Ensure value is set to default
        return false;
    }

    private static bool TryParseValue(string line, string key, out int? value) {
        Match match = Regex.Match(line, $"{key}=(?<value>-?\\d+)");
        if (match.Success) {
            bool parsed = int.TryParse(match.Groups["value"].Value, out int temp);
            value = parsed ? (int?)temp : null;
            return parsed;
        }
        value = null; // Ensure value is set to default
        return false;
    }

    private static bool TryParseStringValue(string line, string key, out string value) {
        Match match = Regex.Match(line, $"{key}=(?<value>[^ ]+)|{key}=\"(?<value>[^\"]+)\"");
        if (match.Success && match.Groups["value"].Success) {
            value = match.Groups["value"].Value;
            return true;
        }
        value = null; // Ensure value is set to default
        return false;
    }

    private static bool TryParseMessage(string line, out string message) {
        Match messageMatch = Regex.Match(line, @"msg=""(?<message>[^""]+)""");
        if (messageMatch.Success) {
            message = messageMatch.Groups["message"].Value;
            return true;
        }
        message = null; // Ensure message is set to default
        return false;
    }

    /// <summary>
    /// Prints the contents of the specified folders in a designated location to the console.
    /// 
    /// This method performs the following tasks:
    /// 1. Establishes the directory paths for the folders to be printed, which may include paths for logs or other resources.
    /// 2. Iterates through each specified folder, retrieving the list of files or subdirectories contained within.
    /// 3. Outputs the contents of each folder to the console, including details such as the names of files and subdirectories.
    /// 4. Handles any exceptions that may occur when accessing the folders (e.g., if a folder does not exist or lacks permission).
    /// 
    /// This method is useful for providing an overview of the files and folders in the specified directories for debugging,
    /// maintenance, or informational purposes.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Thrown if a specified directory path does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if access to a directory is denied due to insufficient permissions.</exception>
    public static void PrintOutFoldersContent() {
        // Define paths for each folder
        string grafanaLogsPath = Path.Combine(AppContext.BaseDirectory, "GrafanaLogs");
        string reportsPath = Path.Combine(AppContext.BaseDirectory, "Reports");

        // Print out the contents of GrafanaLogs
        Console.WriteLine("\nContents of GrafanaLogs folder:");
        if (Directory.Exists(grafanaLogsPath)) {
            string[] grafanaFiles = Directory.GetFiles(grafanaLogsPath);
            bool hasFiles = false; // Flag to check if any files are found

            foreach (string file in grafanaFiles) {
                if (Path.GetExtension(file) != string.Empty) // Check if the file has an extension
                {
                    Console.WriteLine($"- {Path.GetFileName(file)}");
                    hasFiles = true; // Set the flag to true if a file is printed
                }
            }

            // Notify if the folder is empty
            if (!hasFiles) {
                Console.WriteLine("- The GrafanaLogs folder is empty or contains no files with extensions.");
            }
        }
        else {
            Console.WriteLine("GrafanaLogs directory not found.");
        }

        // Print out the contents of Reports
        Console.WriteLine("Contents of Reports folder:");
        if (Directory.Exists(reportsPath)) {
            string[] reportFiles = Directory.GetFiles(reportsPath);
            bool hasReportFiles = false; // Flag to check if any files are found

            foreach (string file in reportFiles) {
                if (Path.GetExtension(file) != string.Empty) // Check if the file has an extension
                {
                    Console.WriteLine($"- {Path.GetFileName(file)}");
                    hasReportFiles = true; // Set the flag to true if a file is printed
                }
            }

            // Notify if the folder is empty
            if (!hasReportFiles) {
                Console.WriteLine("- The Reports folder is empty or contains no files with extensions.");
            }
        }
        else {
            Console.WriteLine("Reports directory not found.");
        }
    }
}