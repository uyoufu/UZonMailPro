using FluentValidation;
using UZonMail.ProPlugin.SQL.EmailCrawler;

namespace UZonMail.ProPlugin.Controllers.EmailCrawler.Validators
{
    public class TikTokDeviceValidator : AbstractValidator<TikTokDevice>
    {
        public TikTokDeviceValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("设备名称不能为空");
            RuleFor(x => x.DeviceId).NotEmpty().WithMessage("设备ID 不能为空");
            RuleFor(x => x.OdinId).NotEmpty().WithMessage("广告ID 不能为空");
        }
    }
}
