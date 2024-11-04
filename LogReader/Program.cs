using System;
using System.Collections.Generic;
using LogReader.Models;

namespace LogReader;

internal class Program {
    static void Main(string[] args) {

        Console.WriteLine("   ---=< GRAFANA LOG READER >=---");

        // Read and print out settings
        ConfigReader.ReadAppSettings();
        ConfigReader.PrintOutAllSettings();

        // Print out GrafanaLogs and Reports folders

        LogsParser.PrintOutFoldersContent();

        // Read content from all log files
        List<string> allLines = LogsParser.ReadLogs();

        // Filter out lines with no useful info
        List<string> goodLines = LogsParser.FilterLogs(allLines);

        // Parst log entries
        List<LogEntry> logEntries = LogsParser.ParseLogs(goodLines);

        // Generate report
        Report report = LogsReporter.GenerateReport(logEntries);
        if (ConfigReader.appParameters.DisplayResults) Console.WriteLine(report.Content);

        // Save report
        LogsReporter.SaveToFile(report);

        if (ConfigReader.appParameters.CloseWhenFinished == false) Console.ReadLine();

    }
}
