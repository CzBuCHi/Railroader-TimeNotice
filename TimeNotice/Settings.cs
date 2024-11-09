using System.Collections.Generic;

namespace TimeNotice;

public class Settings
{
    public List<NotifyInfo> Notifications { get; set; } = new();
}

public class NotifyInfo
{
    public int    Hour    { get; set; }
    public int    Minute  { get; set; }
    public string Message { get; set; } = "";

    public string Time => $"{Hour:D2}:{Minute:D2}";
}
