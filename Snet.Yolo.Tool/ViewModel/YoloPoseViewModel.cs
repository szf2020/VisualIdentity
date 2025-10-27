using SkiaSharp;
using Snet.Core.handler;
using Snet.Model.data;
using Snet.Yolo.Server.handler;
using Snet.Yolo.Server.models.data;
using Snet.Yolo.Server.models.@enum;
using Snet.Yolo.Tool.Data;
using System.IO;
using YoloDotNet.Extensions;
using YoloDotNet.Models;

namespace Snet.Yolo.Tool.ViewModel;

public class YoloPoseViewModel : YoloDetectViewModel
{
    FallDetector fallDetector = new FallDetector();



    /// <summary>
    /// 验证图片
    /// </summary>
    public override async Task V_ImageAsync(ItemsControlBody item, CancellationToken token = default)
    {
        await Task.Run(async () =>
        {
            try
            {

                using SKImage image = SKImage.FromEncodedData(item.Path);
                time.StartRecord();

                OperateResult operateResult = await YoloInit(OnnxType.PoseEstimation).RunAsync(new PoseEstimationData
                {
                    Confidence = Confidence,
                    Iou = Iou,
                    File = image.Encode().ToArray()
                });
                List<PoseEstimation> results = operateResult.GetPoseEstimationResult().ToPoseEstimation();
                List<PoseEstimation> newResults = new List<PoseEstimation>();
                string msg = $"\r\n{App.LanguageOperate.GetLanguageValue("验证")} : {Path.GetFileName(item.Path)}\r\n{App.LanguageOperate.GetLanguageValue("大小")} : {item.Description}\r\n{App.LanguageOperate.GetLanguageValue("用时")} : {time.StopRecord().milliseconds} ms";
                msg += $"\r\n{App.LanguageOperate.GetLanguageValue("目标")} : <{results.Count}> {App.LanguageOperate.GetLanguageValue("个")}";
                if (results.Count > 0)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        msg += $"\r\n<{i + 1}> {App.LanguageOperate.GetLanguageValue("标签")} : {results[i].Label.Name}";
                        msg += $"\r\n<{i + 1}> {App.LanguageOperate.GetLanguageValue("准度")} : {results[i].Confidence}";
                        msg += $"\r\n<{i + 1}> {App.LanguageOperate.GetLanguageValue("坐标")} : {results[i].BoundingBox.ToString()}";
                        if (statistics)
                        {
                            Confidences.Add(results[i].Confidence);
                        }
                        bool fall = fallDetector.IsPersonFallen(FallDetector.ToBodyModelList(results[i].KeyPoints), image.Height);
                        msg += $"\r\n<{i + 1}> 状态 : {(fall ? "跌倒" : "未跌倒")}";
                        if (fall)
                        {
                            newResults.Add(results[i]);
                        }
                    }
                }
                msg += $"\r\n-------------------------------------------------------------------";
                using SKBitmap resultImage = image.Draw(results, new PoseDrawingOptions { KeyPointMarkers = new PoseEstimationCustomKeyPointColorHandler().GetKeyPoints(), PoseConfidence = Confidence, BorderThickness = 3 });   //绘制所有
                //using SKBitmap resultImage = image.Draw(newResults, new PoseDrawingOptions { KeyPointMarkers = CustomKeyPointColorMap.KeyPoints, PoseConfidence = Confidence });  //只绘制跌倒
                ResultImage = await ConvertSKImageToImageSourceAsync(resultImage);
                await msgShow(msg);



                /*
                //Yolo yolo = YoloInit(OnnxModel, ref _needReinit, DeviceType.CUDA);

                //yolo.InitializeVideo(new YoloDotNet.Video.VideoOptions
                //{
                //    // 💡 输入视频源。支持以下格式：
                //    // 
                //    // 1. 本地视频文件：
                //    //    示例：@"C:\videos\test.mp4"
                //    //
                //    // 2. 实时流地址（例如 RTMP、HTTP）：
                //    //    示例："rtmp://your.rtmp.server/stream"
                //    //
                //    // 3. 视频采集设备（例如摄像头）：
                //    //    格式："device=<设备名>:<宽度>:<高度>:<帧率>"
                //    //
                //    //    ⮞ 在 Windows 上：
                //    //       示例："device=Logitech BRIO:1920:1080:30"
                //    //
                //    //    ⮞ 在 Linux 上：
                //    //       示例："device=/dev/video0:1280:720:30"
                //    //
                //    // 📌 宽度、高度和帧率值必须与摄像头支持的采集模式匹配。
                //    //
                //    // 🔍 若要获取可用的视频设备：
                //    //    使用 `yolo.GetVideoDevices()` 方法 —— 该方法列出可用的视频采集设备名称。
                //    //    它**不会**列出设备支持的分辨率或帧率。
                //    //
                //    //    若要确定合法的宽度/高度/帧率组合，请参考设备规格说明。
                //    VideoInput = "device=USB2.0 HD UVC WebCam:1920:1080:30",

                //    // 💡 可选：保存处理后输出视频的路径。
                //    // 若不想保存输出，可设置为 null 或空字符串。
                //    VideoOutput = Path.Combine("video", "video_output.mp4"),

                //    // 💡 输出视频的帧率。
                //    // 使用 FrameRate.AUTO 将尝试匹配输入视频的帧率。
                //    FrameRate = FrameRate.AUTO,

                //    // 💡 输出视频的宽度（单位：像素）。
                //    // 设置为 0 表示使用原始宽度。
                //    // 设置为 -2 表示根据指定的高度保持宽高比自动计算宽度。
                //    // 注意：Width 和 Height 只能有一个设置为 -2。
                //    Width = 720,

                //    // 💡 输出视频的高度（单位：像素）。
                //    // 设置为 0 表示使用原始高度。
                //    // 设置为 -2 表示根据指定的宽度保持宽高比自动计算高度。
                //    // 注意：Width 和 Height 只能有一个设置为 -2。
                //    Height = -2,

                //    // 💡 输出视频的压缩质量（范围：1-51）。
                //    // 数值越低 = 画质越好、文件越大；
                //    // 数值越高 = 压缩越强、文件越小、画质越低。
                //    // 推荐范围：20-35，在质量和体积之间取得平衡。
                //    CompressionQuality = 30,

                //    // 💡 可选：自动将输出视频分段。
                //    // 每段的时长（单位：秒）。
                //    // 示例：600 = 分段为 10 分钟的视频。
                //    // 设置为 0 表示不分段（生成单个文件）。
                //    VideoChunkDuration = 0,

                //    // 💡 每 N 帧处理一次。
                //    // 设置为 0 表示处理所有帧（默认）。
                //    // 示例：30 = 每 30 帧处理一次（适用于对每帧检测要求不高的监控场景）。
                //    FrameInterval = 0

                //});
                //yolo.OnVideoFrameReceived = (SKBitmap frame, long frameIndex) =>
                //{
                //    var result = yolo.RunPoseEstimation(frame, confidence: Confidence, iou: Iou)
                //        .FilterLabels(["person", "cat", "dog"])
                //        .Track(new SortTracker(0.5f, 5, 60));
                //    frame.Draw(result, new PoseDrawingOptions { KeyPointMarkers = CustomKeyPointColorMap.KeyPoints, PoseConfidence = Confidence });

                //    ResultImage = ConvertSKImageToImageSourceAsync(frame).ConfigureAwait(false).ConfigureAwait(false).GetAwaiter().GetResult();
                //};
                //yolo.StartVideoProcessing();
                */
            }
            catch (Exception ex)
            {
                await msgShow($"{App.LanguageOperate.GetLanguageValue("验证图片异常")}:{ex.Message}");
            }
        }, token);
    }
}

/// <summary>
/// 人体跌倒检测器：通过关键点判断人体是否可能跌倒
/// </summary>
public class FallDetector
{
    /// <summary>
    /// 人体关键部位定义（按模型索引顺序）
    /// </summary>
    public enum Body
    {
        鼻子, 左眼, 右眼, 左耳, 右耳,
        左肩, 右肩, 左肘, 右肘, 左腕, 右腕,
        左髋关节, 右髋关节, 左膝, 右膝盖, 左脚踝, 右脚踝
    }

    /// <summary>
    /// 关键点与身体部位映射模型
    /// </summary>
    public class BodyModel
    {
        public KeyPoint Point { get; set; }
        public Body Body { get; set; }
    }

    /// <summary>
    /// 跌倒检测的可配置参数
    /// </summary>
    public class FallDetectionOptions
    {
        /// <summary>
        /// 关键点最小置信度
        /// </summary>
        public float MinConfidence { get; set; } = 0.3f;
        /// <summary>
        /// 身体高度 < 50%图像高度，视为“躺下”
        /// </summary>
        public float FlatHeightRatio { get; set; } = 0.5f;
        /// <summary>
        /// 身体夹角 < 60度，视为“水平”
        /// </summary>
        public float AngleThreshold { get; set; } = 70f;
        /// <summary>
        /// 肩部与臀部Y差值 < 10%图像高度
        /// </summary>
        public float TorsoHorizontalThresholdRatio { get; set; } = 0.1f;
        /// <summary>
        /// 平均Y位置 > 60%图像高度，认为接近地面
        /// </summary>
        public float GroundProximityRatio { get; set; } = 0.6f;
        /// <summary>
        /// 满足几项条件视为跌倒
        /// </summary>
        public int FallScoreThreshold { get; set; } = 2;
    }

    private readonly FallDetectionOptions _options;

    public FallDetector(FallDetectionOptions options = null)
    {
        _options = options ?? new FallDetectionOptions();
    }

    /// <summary>
    /// 将原始关键点转换为标注身体部位的模型列表
    /// </summary>
    public static List<BodyModel> ToBodyModelList(IEnumerable<KeyPoint> model)
    {
        if (model == null) return new List<BodyModel>();

        var keypoints = model.ToArray();
        var bodyParts = Enum.GetValues<Body>();
        int count = Math.Min(keypoints.Length, bodyParts.Length);

        var result = new List<BodyModel>(count);
        for (int i = 0; i < count; i++)
        {
            result.Add(new BodyModel
            {
                Point = keypoints[i],
                Body = bodyParts[i]
            });
        }
        return result;
    }

    /// <summary>
    /// 判断当前人体是否处于“跌倒”状态
    /// </summary>
    public bool IsPersonFallen(List<BodyModel> keypoints, float imageHeight)
    {
        if (keypoints == null || keypoints.Count < 17) return false;

        // 构建 Body 到 Point 的字典，提升效率
        var pointDict = keypoints
            .Where(b => b?.Point != null)
            .GroupBy(b => b.Body)
            .ToDictionary(g => g.Key, g => g.First().Point);

        // 快捷获取函数
        bool TryGet(Body body, out KeyPoint p) => pointDict.TryGetValue(body, out p) && IsConfident(p);

        // 提取并验证所需关键点
        if (!(TryGet(Body.鼻子, out var nose) &&
              TryGet(Body.左肩, out var leftShoulder) &&
              TryGet(Body.右肩, out var rightShoulder) &&
              TryGet(Body.左髋关节, out var leftHip) &&
              TryGet(Body.右髋关节, out var rightHip) &&
              TryGet(Body.左脚踝, out var leftAnkle) &&
              TryGet(Body.右脚踝, out var rightAnkle)))
            return false;

        // 计算核心点
        var shoulderCenter = Average(leftShoulder, rightShoulder);
        var hipCenter = Average(leftHip, rightHip);
        var ankleCenter = Average(leftAnkle, rightAnkle);

        // 1. 身体高度判断
        float bodyHeight = Distance(nose, ankleCenter);
        bool isFlatByHeight = bodyHeight < _options.FlatHeightRatio * imageHeight;

        // 2. 身体角度判断（肩-髋）
        double bodyAngle = GetAngle(shoulderCenter, hipCenter);
        bool isFlatByAngle = bodyAngle < _options.AngleThreshold;

        // 3. 躯干是否水平（Y方向接近）
        float torsoYDiff = Math.Abs(shoulderCenter.Y - hipCenter.Y);
        bool isTorsoHorizontal = torsoYDiff < _options.TorsoHorizontalThresholdRatio * imageHeight;

        // 所有可信关键点的平均 Y 坐标是否较靠下
        float avgY = (float)keypoints
            .Where(k => IsConfident(k.Point))
            .Average(k => k.Point.Y);
        bool isNearGround = avgY > _options.GroundProximityRatio * imageHeight;

        // 评分统计（满足几项）
        int fallScore = 0;
        if (isFlatByHeight) fallScore++;
        if (isFlatByAngle) fallScore++;
        if (isTorsoHorizontal) fallScore++;
        if (isNearGround) fallScore++;

        return fallScore >= _options.FallScoreThreshold;
    }

    /// <summary>
    /// 判断关键点是否可信
    /// </summary>
    private bool IsConfident(KeyPoint point)
    {
        return point != null && point.Confidence >= _options.MinConfidence;
    }

    /// <summary>
    /// 计算两个关键点的中点坐标
    /// </summary>
    private (float X, float Y) Average(KeyPoint a, KeyPoint b)
    {
        if (a == null || b == null) return (0, 0);
        return ((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);
    }

    /// <summary>
    /// 计算两点间的欧几里得距离
    /// </summary>
    private float Distance(KeyPoint a, (float X, float Y) b)
    {
        if (a == null) return 0f;
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 计算两点连线与水平线夹角（0~90°）
    /// </summary>
    private double GetAngle((float X, float Y) a, (float X, float Y) b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        if (Math.Abs(dx) < 1e-5 && Math.Abs(dy) < 1e-5)
            return 90; // 避免除以0

        return Math.Atan2(Math.Abs(dy), Math.Abs(dx)) * 180.0 / Math.PI;
    }
}





