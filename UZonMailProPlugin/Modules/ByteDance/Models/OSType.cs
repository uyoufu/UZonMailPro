using System.Runtime.Serialization;

namespace UZonMailProPlugin.Modules.ByteDance.Models
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
