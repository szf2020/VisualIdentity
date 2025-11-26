using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Snet.Model.data;
using Snet.Utility;
using Snet.Yolo.Api.Attribute;
using Snet.Yolo.Api.Handler;
using Snet.Yolo.Api.Model;
using Snet.Yolo.Server;
using Snet.Yolo.Server.handler;
using Snet.Yolo.Server.@interface;
using Snet.Yolo.Server.models.data;
using Snet.Yolo.Server.models.@enum;
using YoloDotNet.Core;
using YoloDotNet.Extensions;
using YoloDotNet.Models;

namespace Snet.Yolo.Api.Controllers
{
    /// <summary>
    /// 操作
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class OperateController : ControllerBase
    {
        /// <summary>
        /// 管理操作
        /// </summary>
        private ManageOperate _operate;
        /// <summary>
        /// 配置
        /// </summary>
        private ConfigModel _config;
        /// <summary>
        /// 姿态处理
        /// </summary>
        private PoseEstimationCustomKeyPointColorHandler _poseHandler;
        /// <summary>
        /// 操作控制器<br/>
        /// 有参构造函数
        /// </summary>
        /// <param name="operate">管理操作</param>
        /// <param name="config">配置</param>
        public OperateController(ManageOperate operate, IOptions<ConfigModel> config, PoseEstimationCustomKeyPointColorHandler poseHandler)
        {
            _operate = operate;
            _config = config.Value;
            _poseHandler = poseHandler;
        }

        /// <summary>
        /// 识别
        /// </summary>
        /// <param name="onnxIndex">数据库模型下标</param>
        /// <param name="file">识别的文件</param>
        /// <param name="paramJson">识别基础属性<br/>
        /// Classification：{"Classes":1}:分类数据<br/>
        /// ObbDetection：{"Confidence":0.2,"Iou":0.7}:定向检测数据<br/>
        /// ObjectDetection：{"Confidence":0.2,"Iou":0.7}:检测数据<br/>
        /// PoseEstimation：{"Confidence":0.2,"Iou":0.7}:姿态识别数据<br/>
        /// Segmentation：{"Confidence":0.2,"Iou":0.7,"PixelConfedence":0.65}:分割数据</param>
        /// <param name="hardwareJson">使用什么硬件来进行运算<br/>
        /// CPU：{"CpuExecutionProvider":{}}<br/>
        /// CUDA：{"CudaExecutionProvider":{"GpuId":0,"PrimeGpu":false}}<br/>
        /// NVIDIA：{"TensorRtExecutionProvider":{"Precision":0,"GpuId":0,"BuilderOptimizationLevel":3,"EngineCachePath":null,"Int8CalibrationCacheFile":null,"EngineCachePrefix":null}}
        /// </param>
        /// <returns>
        /// 识别结果<br/>
        /// 返回识别到的坐标数据
        /// </returns>
        [HttpPost]
        public async Task<OperateResult> IdentityAsync(int onnxIndex, [AllowedFileType([".jpg", ".jpeg", ".png", ".bmp"])] IFormFile file, string paramJson, string hardwareJson = "{\"CpuExecutionProvider\":{}}")
        {
            OperateResult result = await QueryAsync(onnxIndex);
            if (result.GetDetails(out List<OnnxData>? datas))
            {
                OnnxData onnxData = datas[0];
                IdentityOperate operate = IdentityOperate.Instance(new IdentityData
                {
                    SN = $"{PublicHandler.DefaultSN}-{hardwareJson}",
                    OnnxPath = Path.Combine(onnxData.path, onnxData.name),
                    Hardware = hardwareJson.ToJsonEntity<HardwareData>()?.GetHardware() ?? new CpuExecutionProvider(),
                    IdentifyType = onnxData.onnxType ??= OnnxType.ObjectDetection,
                });

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                byte[] bytes = ms.ToArray();

                IData data = null;
                switch (onnxData.onnxType ??= OnnxType.ObjectDetection)
                {
                    case OnnxType.ObjectDetection:
                        ObjectDetectionData objectDetection = paramJson.ToJsonEntity<ObjectDetectionData>();
                        objectDetection.File = bytes;
                        data = objectDetection;
                        break;
                    case OnnxType.Segmentation:
                        SegmentationData segmentation = paramJson.ToJsonEntity<SegmentationData>();
                        segmentation.File = bytes;
                        data = segmentation;
                        break;
                    case OnnxType.Classification:
                        ClassificationData classification = paramJson.ToJsonEntity<ClassificationData>();
                        classification.File = bytes;
                        data = classification;
                        break;
                    case OnnxType.PoseEstimation:
                        PoseEstimationData poseEstimation = paramJson.ToJsonEntity<PoseEstimationData>();
                        poseEstimation.File = bytes;
                        data = poseEstimation;
                        break;
                    case OnnxType.ObbDetection:
                        ObbDetectionData obbDetection = paramJson.ToJsonEntity<ObbDetectionData>();
                        obbDetection.File = bytes;
                        data = obbDetection;
                        break;
                }
                return await operate.RunAsync(data);
            }
            return result;
        }

        /// <summary>
        /// 识别返回依据坐标数据处理完成的绘制后图片包含坐标数据<br/>
        /// 注意:绘制将占用大量时间
        /// </summary>
        /// <param name="onnxIndex">数据库模型下标</param>
        /// <param name="file">识别的文件</param>
        /// <param name="paramJson">识别基础属性<br/>
        /// Classification：{"Classes":1}:分类数据<br/>
        /// ObbDetection：{"Confidence":0.2,"Iou":0.7}:定向检测数据<br/>
        /// ObjectDetection：{"Confidence":0.2,"Iou":0.7}:检测数据<br/>
        /// PoseEstimation：{"Confidence":0.2,"Iou":0.7}:姿态识别数据<br/>
        /// Segmentation：{"Confidence":0.2,"Iou":0.7,"PixelConfedence":0.65}:分割数据</param>
        /// <param name="hardwareJson">使用什么硬件来进行运算<br/>
        /// CPU：{"CpuExecutionProvider":{}}<br/>
        /// CUDA：{"CudaExecutionProvider":{"GpuId":0,"PrimeGpu":false}}<br/>
        /// NVIDIA：{"TensorRtExecutionProvider":{"Precision":0,"GpuId":0,"BuilderOptimizationLevel":3,"EngineCachePath":null,"Int8CalibrationCacheFile":null,"EngineCachePrefix":null}}
        /// </param>
        /// <returns>
        /// 识别结果<br/>
        /// 绘制后图片包含坐标数据
        /// </returns>
        [HttpPost]
        public async Task<OperateResult> IdentityDrawAsync(int onnxIndex, [AllowedFileType([".jpg", ".jpeg", ".png", ".bmp"])] IFormFile file, string paramJson, string hardwareJson = "{\"CpuExecutionProvider\":{}}")
        {
            OperateResult result = await QueryAsync(onnxIndex);
            if (result.GetDetails(out List<OnnxData>? datas))
            {
                string ms = DateTime.Now.ToString(_config.NameFormat);
                TimeHandler.Instance(ms).StartRecord();

                OnnxData onnxData = datas[0];
                IdentityOperate operate = IdentityOperate.Instance(new IdentityData
                {
                    SN = $"{PublicHandler.DefaultSN}-{hardwareJson}",
                    OnnxPath = Path.Combine(onnxData.path, onnxData.name),
                    Hardware = hardwareJson.ToJsonEntity<HardwareData>()?.GetHardware() ?? new CpuExecutionProvider(),
                    IdentifyType = onnxData.onnxType ??= OnnxType.ObjectDetection,
                });

                string suffix = file.GetSuffix();
                byte[] imageBytes = await file.GetBytesAsync();
                using SKImage image = SKImage.FromEncodedData(imageBytes);

                switch (onnxData.onnxType ??= OnnxType.ObjectDetection)
                {
                    case OnnxType.ObjectDetection:
                        ObjectDetectionData objectDetection = paramJson.ToJsonEntity<ObjectDetectionData>();
                        objectDetection.File = imageBytes;
                        result = await operate.RunAsync(objectDetection);
                        if (result.GetDetails(out List<ObjectDetectionResultData>? objectDetectionResultDatas))
                        {
                            if (objectDetectionResultDatas.Count > 0)
                            {
                                List<ObjectDetection> datasResult = objectDetectionResultDatas.ToObjectDetection();
                                using SKBitmap sKBitmap = image.Draw(datasResult);
                                byte[] ibytes = sKBitmap.GteImageByte(out string contentType);
                                string name = ImageHandler.SaveImage(ibytes, imageBytes, objectDetectionResultDatas, onnxData.onnxType.Value, _config);
                                string url = Url.Action("GetImage", "Operate", new { name = name, type = onnxData.onnxType.Value }, Request.Scheme);
                                return OperateResult.CreateSuccessResult("Identity Success", new IdentityResultData<List<ObjectDetectionResultData>>(objectDetectionResultDatas, url), TimeHandler.Instance(ms).StopRecord().milliseconds);
                            }
                        }
                        return OperateResult.CreateFailureResult("Identity Failure", TimeHandler.Instance(ms).StopRecord().milliseconds);
                    case OnnxType.Segmentation:
                        SegmentationData segmentation = paramJson.ToJsonEntity<SegmentationData>();
                        segmentation.File = imageBytes;
                        result = await operate.RunAsync(segmentation);
                        if (result.GetDetails(out List<SegmentationResultData>? segmentationDatas))
                        {
                            if (segmentationDatas.Count > 0)
                            {
                                List<Segmentation> datasResult = segmentationDatas.ToSegmentation();
                                using SKBitmap sKBitmap = image.Draw(datasResult);
                                byte[] ibytes = sKBitmap.GteImageByte(out string contentType);
                                string name = ImageHandler.SaveImage(ibytes, imageBytes, segmentationDatas, onnxData.onnxType.Value, _config);
                                string url = Url.Action("GetImage", "Operate", new { name = name, type = onnxData.onnxType.Value }, Request.Scheme);
                                return OperateResult.CreateSuccessResult("Identity Success", new IdentityResultData<List<SegmentationResultData>>(segmentationDatas, url), TimeHandler.Instance(ms).StopRecord().milliseconds);
                            }
                        }
                        return OperateResult.CreateFailureResult("Identity Failure", TimeHandler.Instance(ms).StopRecord().milliseconds);
                    case OnnxType.Classification:
                        ClassificationData classification = paramJson.ToJsonEntity<ClassificationData>();
                        classification.File = imageBytes;
                        result = await operate.RunAsync(classification);
                        if (result.GetDetails(out List<ClassificationResultData>? classificationDatas))
                        {
                            if (classificationDatas.Count > 0)
                            {
                                List<Classification> datasResult = classificationDatas.ToClassification();
                                using SKBitmap sKBitmap = image.Draw(datasResult);
                                byte[] ibytes = sKBitmap.GteImageByte(out string contentType);
                                string name = ImageHandler.SaveImage(ibytes, imageBytes, classificationDatas, onnxData.onnxType.Value, _config);
                                string url = Url.Action("GetImage", "Operate", new { name = name, type = onnxData.onnxType.Value }, Request.Scheme);
                                return OperateResult.CreateSuccessResult("Identity Success", new IdentityResultData<List<ClassificationResultData>>(classificationDatas, url), TimeHandler.Instance(ms).StopRecord().milliseconds);
                            }
                        }
                        return OperateResult.CreateFailureResult("Identity Failure", TimeHandler.Instance(ms).StopRecord().milliseconds);
                    case OnnxType.PoseEstimation:
                        PoseEstimationData poseEstimation = paramJson.ToJsonEntity<PoseEstimationData>();
                        poseEstimation.File = imageBytes;
                        result = await operate.RunAsync(poseEstimation);
                        if (result.GetDetails(out List<PoseEstimationResultData>? poseEstimationDatas))
                        {
                            if (poseEstimationDatas.Count > 0)
                            {
                                List<PoseEstimation> datasResult = poseEstimationDatas.ToPoseEstimation();
                                using SKBitmap sKBitmap = image.Draw(datasResult, new PoseDrawingOptions { KeyPointMarkers = _poseHandler.GetKeyPoints(), PoseConfidence = poseEstimation.Confidence, BorderThickness = 3 });
                                byte[] ibytes = sKBitmap.GteImageByte(out string contentType);
                                string name = ImageHandler.SaveImage(ibytes, imageBytes, poseEstimationDatas, onnxData.onnxType.Value, _config);
                                string url = Url.Action("GetImage", "Operate", new { name = name, type = onnxData.onnxType.Value }, Request.Scheme);
                                return OperateResult.CreateSuccessResult("Identity Success", new IdentityResultData<List<PoseEstimationResultData>>(poseEstimationDatas, url), TimeHandler.Instance(ms).StopRecord().milliseconds);
                            }
                        }
                        return OperateResult.CreateFailureResult("Identity Failure", TimeHandler.Instance(ms).StopRecord().milliseconds);
                    case OnnxType.ObbDetection:
                        ObbDetectionData obbDetection = paramJson.ToJsonEntity<ObbDetectionData>();
                        obbDetection.File = imageBytes;
                        result = await operate.RunAsync(obbDetection);
                        if (result.GetDetails(out List<ObbDetectionResultData>? obbDetections))
                        {
                            if (obbDetections.Count > 0)
                            {
                                List<OBBDetection> datasResult = obbDetections.ToObbDetection();
                                using SKBitmap sKBitmap = image.Draw(datasResult);
                                byte[] ibytes = sKBitmap.GteImageByte(out string contentType);
                                string name = ImageHandler.SaveImage(ibytes, imageBytes, obbDetections, onnxData.onnxType.Value, _config);
                                string url = Url.Action("GetImage", "Operate", new { name = name, type = onnxData.onnxType.Value }, Request.Scheme);
                                return OperateResult.CreateSuccessResult("Identity Success", new IdentityResultData<List<ObbDetectionResultData>>(obbDetections, url), TimeHandler.Instance(ms).StopRecord().milliseconds);
                            }
                        }
                        return OperateResult.CreateFailureResult("Identity Failure", TimeHandler.Instance(ms).StopRecord().milliseconds);
                }
            }
            return result;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="file">文件</param>
        /// <param name="describe">描述</param>
        /// <param name="onnxType">模型类型</param>
        /// <returns>结果</returns>
        [HttpPost]
        public async Task<OperateResult> AddAsync([AllowedFileType([".onnx"])] IFormFile file, string describe, OnnxType onnxType)
        {
            var savePath = Path.Combine(PublicHandler.DefaultPath, "onnxs");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            var filePath = Path.Combine(savePath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            OperateResult result = await _operate.AddAsync(filePath, describe, onnxType);
            if (!result.Status)
            {
                System.IO.File.Delete(filePath);
            }
            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="describe">描述</param>
        /// <param name="onnxType">类型</param>
        /// <returns>结果</returns>
        [HttpPost]
        public async Task<OperateResult> UpdateAsync(int index, string describe, OnnxType? onnxType = null) => await _operate.UpdateAsync(index, describe, onnxType);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="index">下标</param>
        /// <param name="deleteFile">是否删除文件</param>
        /// <returns>结果</returns>
        [HttpPost]
        public async Task<OperateResult> DeleteAsync(int index, bool deleteFile = true) => await _operate.DeleteAsync(index, deleteFile);

        /// <summary>
        /// 指定查询
        /// </summary>
        /// <param name="index">下标</param>
        /// <returns>结果</returns>
        [HttpGet]
        public async Task<OperateResult> QueryAsync(int index) => await _operate.QueryAsync(index);

        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns>结果</returns>
        [HttpGet]
        public async Task<OperateResult> QuerysAsync() => await _operate.QueryAsync();

        /// <summary>
        /// 获取本地图片
        /// </summary>
        /// <param name="name">
        /// 图片名称（文件名的一部分，不包含扩展名）
        /// </param>
        /// <param name="type">
        /// 模型类型（用于定位子目录）
        /// </param>
        /// <returns>
        /// 成功时返回图片文件，失败时返回错误信息
        /// </returns>
        [HttpGet]
        public IActionResult GetImage(string name, OnnxType type)
        {
            // 参数校验：name 不能为空
            if (string.IsNullOrEmpty(name))
                return BadRequest("Parameter 'name' cannot be null or empty.");

            // 拼接目录路径：BasePath/yyyy-MM-dd/OnnxType
            string directory = Path.Combine(_config.BasePath, DateTime.Now.ToString("yyyy-MM-dd"), type.ToString());

            // 判断目录是否存在
            if (!Directory.Exists(directory))
                return NotFound("Target directory does not exist.");

            // 获取目录下的所有文件
            string[] files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);

            // 按照配置规则格式化目标文件名
            string expectedFileName = string.Format(_config.ResultImageNamingFormat, name);

            // 查找第一个匹配的文件
            string path = files.FirstOrDefault(f => Path.GetFileName(f).Contains(expectedFileName));

            // 校验文件是否存在
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return NotFound("Target file not found.");

            // 读取文件字节数据
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            // 以 image/jpeg 格式返回图片
            return File(bytes, "image/jpeg");
        }

    }
}
