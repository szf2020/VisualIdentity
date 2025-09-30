using SkiaSharp;
using Snet.Utility;
using Snet.Yolo.Api.Model;
using Snet.Yolo.Server.models.@enum;

namespace Snet.Yolo.Api.Handler
{
    /// <summary>
    /// 图片处理
    /// </summary>
    public static class ImageHandler
    {
        /// <summary>
        /// 获取图片字节数组
        /// </summary>
        /// <param name="file">表单文件对象</param>
        /// <returns>字节数据</returns>
        public static async Task<byte[]> GetBytesAsync(this IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }


        /// <summary>
        /// 获取后缀名
        /// </summary>
        /// <param name="file">表单文件对象</param>
        /// <param name="containsSymbols">是否包含符号</param>
        /// <returns>后缀名</returns>
        public static string GetSuffix(this IFormFile file, bool containsSymbols = false)
        {
            string extension = Path.GetExtension(file.FileName);
            if (containsSymbols)
            {
                return extension;
            }
            return extension.TrimStart('.');
        }

        /// <summary>
        /// 获取图片字节
        /// </summary>
        /// <param name="sKBitmap">源</param>
        /// <param name="contentType">内容类型</param>
        /// <returns>图片字节</returns>
        public static byte[] GteImageByte(this SKBitmap sKBitmap, out string contentType)
        {
            using var resultImage = SKImage.FromBitmap(sKBitmap);
            using var encoded = resultImage.Encode(SKEncodedImageFormat.Png, 100);
            contentType = "image/png";
            return encoded.ToArray();
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <typeparam name="T">泛型数据结果对象</typeparam>
        /// <param name="result">结果图数据</param>
        /// <param name="origina">原图数据</param>
        /// <param name="data">结果数据</param>
        /// <param name="type">检查类型</param>
        /// <param name="configModel">配置</param>
        /// <returns></returns>
        public static string SaveImage<T>(byte[] result, byte[] origina, T data, OnnxType type, ConfigModel configModel)
        {
            string directory = Path.Combine(configModel.BasePath, DateTime.Now.ToString("yyyy-MM-dd"), type.ToString());
            string name = DateTime.Now.ToString(configModel.NameFormat);
            string _result = Path.Combine(directory, string.Format(configModel.ResultImageNamingFormat, name));
            string _details = Path.Combine(directory, string.Format(configModel.DetailsNamingFormat, name));
            string _original = Path.Combine(directory, string.Format(configModel.OriginalImageNamingFormat, name));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(_result, result);
            File.WriteAllText(_details, data.ToJson(true));
            File.WriteAllBytes(_original, origina);
            return name;
        }
    }
}
