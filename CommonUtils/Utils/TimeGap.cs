namespace CommonUtils.Utils
{
    public class TimeGap
    {
        public DateTime GapTime { get; set; }
        public TimeSpan MissingSpan { get; set; }
        public override string ToString() => $"{GapTime} - {Math.Round(MissingSpan.TotalMinutes,2)} minutes missing";

    }
}
