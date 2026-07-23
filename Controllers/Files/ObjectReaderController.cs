using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Files;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.ProPlugin.Controllers.Base;

namespace UZonMail.ProPlugin.Controllers.Files
{
    public class ObjectReaderController(SqlContext db) : ControllerBasePro
    {
        private static FileExtensionContentTypeProvider _contentTypeProvider = new();

        [Authorize(Roles = "Professional,Enterprise")]
        [HttpGet("persistent")]
        public async Task<ResponseResult<string>> CreateObjectPersistentReader(long fileUsageId)
        {
            // 查找文件对象
            var fileObject = await db.FileUsages.Where(x => x.Id == fileUsageId)
                .FirstOrDefaultAsync();
            if (fileObject == null) return string.Empty.ToFailResponse("文件不存在");

            // 判断是否存在 reader
            var existReader = await db.FileReaders.Where(x => x.FileObjectId == fileObject.FileObjectId).FirstOrDefaultAsync();
            if (existReader == null)
            {
                // 生成临时读取链接
                existReader = new FileReader(fileObject);
                db.FileReaders.Add(existReader);
            }

            existReader.ExpireDate = DateTime.UtcNow.AddYears(100);
            await db.SaveChangesAsync();

            return existReader.ObjectId.ToSuccessResponse();
        }

        [HttpGet("stream/{readerObjectId:length(24)}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStream(string readerObjectId)
        {
            // 查找文件对象
            var fileReader = await db.FileReaders.Where(x => x.ObjectId == readerObjectId)
                .Include(x => x.FileObject)
                .ThenInclude(x => x.FileBucket)
                .FirstOrDefaultAsync();

            if (fileReader == null) return NotFound("未找到该文件");
            if (fileReader.FileObject == null) return BadRequest("文件已删除");

            // 保存访问次数
            fileReader.VisitedCount += 1;
            if(fileReader.FirstDate>DateTime.UtcNow)
            {
                fileReader.FirstDate = DateTime.UtcNow;
            }
            fileReader.LastDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // 判断是否过期
            if (fileReader.ExpireDate < DateTime.UtcNow)
            {
                db.FileReaders.Remove(fileReader);
                await db.SaveChangesAsync();

                return NotFound("文件已过期");
            }

            string fullPath = Path.Combine(fileReader.FileObject.FileBucket.RootDir, fileReader.FileObject.Path);
            var fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists) return NotFound("原始文件已删除");

            string contentType = "application/octet-stream";
            if (_contentTypeProvider.TryGetContentType(fileInfo.Name, out var value))
            {
                contentType = value;
            }
            var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            Response.Headers.Append("Content-Disposition", "inline");
            return File(stream, contentType);
        }
    }
}
