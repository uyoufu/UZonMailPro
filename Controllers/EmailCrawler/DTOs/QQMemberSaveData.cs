namespace UZonMail.ProPlugin.Controllers.EmailCrawler.DTOs
{
    public class QQMemberSaveData
    {
        /// <summary>
        /// QQ 群信息
        /// </summary>
        public QQGroupData Group { get; set; }

        /// <summary>
        /// QQ 群成员列表
        /// </summary>
        public List<QQUserData> Users { get; set; }
    }
}
