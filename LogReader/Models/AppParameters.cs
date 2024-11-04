namespace LogReader.Models;
#nullable disable
public class AppParameters {
    public string LogFolder { get; set; }
    public string ReportFolder { get; set; }
    public string ReportFileNameFormat { get; set; }
    public bool DisplayResults { get; set; }
    public bool CloseWhenFinished { get; set; }
}
