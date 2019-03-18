using System.Linq;

public class Title
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public int VideoId => int.Parse(Url.Split('/').LastOrDefault());
    public TitleStatus Status { get; set; }
    public string VideoKey { get; set; }
    public string SubtitleKey { get; set; }
    public QualificationStatus Qulalification { get; set; }
    public string AgeRating { get; set; }
    public string ProductionYear { get; set; }

    //ef


    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || GetType() != obj.GetType())
        {
            return false;
        }

        var p = (Title)obj;
        return Id == p.Id;
    }

}
public enum QualificationStatus
{
    NoData,
    Ok,
    VideoNotEnglish,
    SubtitleNotMatching,
    VideoNotAvailable
}
public enum TitleStatus
{
    AwaitingSubtitle = 0,
    AwaitingVideo = 1,
    Ignored = 2,
    Generating = 3,
    Generated = 4,
    Downloading = 5,
    Queued = 6,
    Error = 7
}