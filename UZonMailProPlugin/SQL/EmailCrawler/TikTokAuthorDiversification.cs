﻿using UZonMail.DB.SQL.Base;

namespace UZonMailProPlugin.SQL.EmailCrawler
{
    /// <summary>
    /// tiktok 用户视频所属类别
    /// 通过这个类别可以大概了解用户的视频类型
    /// </summary>
    public class TikTokAuthorDiversification : SqlId
    {
        /// <summary>
        /// 用户 id
        /// </summary>
        public long TikTokAuthorId { get; set; }

        /// <summary>
        /// 分类 id
        /// </summary>
        public long DiversificationId { get; set; }
    }
}
