using System;

namespace LogReader.Models;
#nullable disable
public class Report {
    // Field to hold the text of the report
    public string Content { get; set; }

    // The newest timestamp found
    public DateTime NewestTimestamp { get; set; }

}