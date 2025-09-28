using SkiaSharp;
using Snet.Model.data;
using Snet.Utility;
using Snet.Yolo.Server;
using Snet.Yolo.Server.handler;
using Snet.Yolo.Server.models.data;
using Snet.Yolo.Server.models.@enum;
using YoloDotNet.Core;
using YoloDotNet.Extensions;
using YoloDotNet.Models;

namespace Snet.Yolo.Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //????? 为对应数据

            // 原始图片路径
            string imagePath = "?????";

            #region 如何把API过来的Json 数据反序列化成对象,并实现绘制功能
            //如何把API过来的Json 数据反序列化成对象,并实现绘制功能
            //以检测功能为例

            // json 数据
            string apiJosn = "?????";

            //转换结果
            List<ObjectDetection> results = apiJosn.ToJsonEntity<List<ObjectDetectionResultData>>().ToObjectDetection();

            //加载图片
            using SKImage image = SKImage.FromEncodedData(imagePath);

            //绘制结果
            using SKBitmap resultImage = image.Draw(results);
            #endregion

            #region 本地调用识别并绘制
            //本地调用识别并绘制

            //模型路径
            string onnxModel = "?????";

            //识别类型
            OnnxType onnxType = OnnxType.ObjectDetection;

            //直接调用库来进行本地识别操作
            using SKImage image2 = SKImage.FromEncodedData(imagePath);

            // 调用识别
            OperateResult operateResult = await IdentityOperate.Instance(new Yolo.Server.models.data.IdentityData
            {
                Hardware = new CpuExecutionProvider(),
                IdentifyType = onnxType,
                OnnxPath = onnxModel,
                SN = $"{onnxType}{onnxModel}"
            }).RunAsync(new ObjectDetectionData
            {
                Confidence = 0.23,
                Iou = 0.7,
                File = image2.Encode().ToArray()
            });

            // 转换结果
            List<ObjectDetection> results2 = operateResult.GetObjectDetectionResult().ToObjectDetection();

            //绘制结果
            using SKBitmap resultImage2 = image2.Draw(results2);
            #endregion

        }
    }
}
