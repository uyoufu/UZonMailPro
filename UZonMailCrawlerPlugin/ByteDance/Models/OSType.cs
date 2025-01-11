using System.Runtime.Serialization;

namespace UZonMailCrawlerPlugin.ByteDance.Models
{
    public enum OSType
    {
        [EnumMember(Value ="windows")]
        Windows,
        MacOS,
        Linux,
        Android,
        iOS,
        Unknown
    }
}
