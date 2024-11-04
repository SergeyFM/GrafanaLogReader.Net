using System;

namespace LogReader.Models;
#nullable disable
public class LogEntry {
    // The time when the log entry was created
    public DateTime Timestamp { get; set; }

    // The user ID associated with the log entry
    public int UserId { get; set; }

    // The organization ID associated with the log entry
    public int OrgId { get; set; }

    // The username of the person associated with the log entry
    public string Username { get; set; }

    // The log level (e.g., info, error, etc.)
    public string LogLevel { get; set; }

    // The message of the log entry
    public string Message { get; set; }

    // The request method associated with the log entry (e.g., GET, POST)
    public string RequestMethod { get; set; }

    // The request path associated with the log entry
    public string RequestPath { get; set; }

    // The status of the request
    public int? Status { get; set; } // Nullable to accommodate logs that may not always include status

    // The remote address of the client making the request
    public string RemoteAddress { get; set; }

    // The time taken to process the request in milliseconds
    public long? TimeMs { get; set; } // Nullable for the same reason as above

    // The size of the response payload
    public int? Size { get; set; } // Nullable for the same reason as above

    // The referer of the request
    public string Referer { get; set; }

    // Additional entries can be extracted similarly based on the context
    // For example, parser, plugin ID, etc. can also be considered
    public string PluginId { get; set; }
    public string Type { get; set; }

    // Field to hold parsing errors
    public string ParsingErrors { get; set; }

    // Override ToString method to provide a string representation of the LogEntry
    public override string ToString() {
        return $"Timestamp: {Timestamp}, " +
               $"UserId: {UserId}, " +
               $"OrgId: {OrgId}, " +
               $"Username: {Username}, " +
               $"LogLevel: {LogLevel}, " +
               $"Message: \"{Message}\", " +
               $"RequestMethod: {RequestMethod}, " +
               $"RequestPath: {RequestPath}, " +
               $"Status: {Status}, " +
               $"RemoteAddress: {RemoteAddress}, " +
               $"TimeMs: {TimeMs}, " +
               $"Size: {Size}, " +
               $"Referer: \"{Referer}\", " +
               $"PluginId: {PluginId}, " +
               $"Type: {Type}, " +
               $"ParsingErrors: \"{ParsingErrors}\"";
    }
}