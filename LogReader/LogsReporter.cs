
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogReader.Models;

namespace LogReader;

public static class LogsReporter {
    public static Report GenerateReport(List<LogEntry> logEntries) {
        // Group by Username
        var groupedEntries = logEntries
            .Where(entry => !string.IsNullOrEmpty(entry.Username)) // Exclude entries without a Username
            .GroupBy(entry => entry.Username)
            .Select(group => new {
                Username = group.Key,
                Entries = group.OrderByDescending(e => e.Timestamp).ToList(),
                FirstActivity = group.Min(e => e.Timestamp),
                LastActivity = group.Max(e => e.Timestamp),
                ActionCount = group.Count()
            })
            .ToList();

        // Prepare the report content
        System.Text.StringBuilder reportContent = new();

        // Statistics section
        reportContent.AppendLine("\nStatistics:\n");
        foreach (var userGroup in groupedEntries) {
            reportContent.AppendLine($"{userGroup.Username}");
            reportContent.AppendLine($"First activity: {userGroup.FirstActivity}");
            reportContent.AppendLine($"Last activity: {userGroup.LastActivity}");
            reportContent.AppendLine($"Number of actions: {userGroup.ActionCount}");
            reportContent.AppendLine();
        }

        // List of activities
        reportContent.AppendLine("\nActivities:\n");
        foreach (var userGroup in groupedEntries) {
            reportContent.AppendLine(userGroup.Username);
            foreach (LogEntry? entry in userGroup.Entries) {
                reportContent.AppendLine($"{entry.Timestamp}, {entry.LogLevel}, {entry.RequestPath}, {entry.Referer}, {entry.Message}");
            }
        }

        // Return the report
        Report report = new() {
            Content = reportContent.ToString(),
            NewestTimestamp = logEntries.Max(e => e.Timestamp)
        };

        return report;
    }

    public static void SaveToFile(Report report) {
        // Inform user about the saving process
        Console.WriteLine("Saving report to file...");

        // Check if the timestamp has a valid value
        if (report.NewestTimestamp == default) {
            Console.WriteLine("Error: NewestTimestamp is not set.");
            return;
        }

        // Format the filename using the ReportFileNameFormat
        // Create the formatted filename based on the NewestTimestamp
        string formattedFileName = CleanFileName(report.NewestTimestamp.ToString(ConfigReader.appParameters.ReportFileNameFormat) + "_report.txt");

        string filePath = Path.Combine(ConfigReader.appParameters.ReportFolder, formattedFileName);

        // Measure time taken to save the file
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Save the content to the file
        File.WriteAllText(filePath, report.Content);

        // Stop the timer
        stopwatch.Stop();

        // Notify user about the completion
        Console.WriteLine($"Report saved as: {filePath} in {stopwatch.Elapsed.TotalMilliseconds} ms");
    }

    public static string CleanFileName(string fileName) {
        // Replace colons with hyphens
        fileName = fileName.Replace(":", "-");

        // Define a list of invalid characters for Windows and Linux
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Remove invalid characters
        foreach (char c in invalidChars)
            fileName = fileName.Replace(c.ToString(), string.Empty);

        // Optionally, remove any additional unwanted characters (like slashes)
        fileName = fileName.Replace("/", "-").Replace("\\", "-"); // Replace slashes with hyphens or you can choose to remove them

        // Trim any leading or trailing whitespace
        fileName = fileName.Trim();

        return fileName;
    }

}
