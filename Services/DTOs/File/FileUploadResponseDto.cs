using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs.File
{
    public class FileUploadResponseDto
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public string FileType { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploaderName { get; set; }
    }
    public class UploadThumbnailDto
    {
        [Required(ErrorMessage = "File thumbnail không được để trống")]
        public IFormFile ThumbnailFile { get; set; }
    }

    /// <summary>
    /// DTO cho file upload của course content
    /// </summary>
    public class UploadContentFileDto
    {
        [Required(ErrorMessage = "File nội dung không được để trống")]
        public IFormFile ContentFile { get; set; }

        /// <summary>
        /// Loại file: video, audio, document
        /// </summary>
        [Required(ErrorMessage = "Loại file không được để trống")]
        public string FileType { get; set; }
    }

    /// <summary>
    /// DTO cho multiple file upload
    /// </summary>
    public class MultipleFileUploadDto
    {
        [Required(ErrorMessage = "Danh sách file không được để trống")]
        public List<IFormFile> Files { get; set; } = new();

        public string? FileType { get; set; }
        public string? Description { get; set; }
    }
}
