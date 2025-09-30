using System.ComponentModel;

namespace Snet.Yolo.Server.models.data
{
    /// <summary>
    /// 识别结果数据
    /// </summary>
    public class IdentityResultData<T>
    {
        /// <summary>
        /// 识别构造函数
        /// </summary>
        /// <param name="result">结果数据</param>
        /// <param name="markImage">标记图片</param>
        public IdentityResultData(T result, string markImage)
        {
            this.Result = result;
            this.MarkImage = markImage;
        }
        /// <summary>
        /// 结果数据
        /// </summary>
        [Description("结果数据")]
        public T Result { get; set; }
        /// <summary>
        /// 标记图片
        /// </summary>
        [Description("标记图片")]
        public string MarkImage { get; set; }
    }
}
