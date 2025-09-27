# <img src="https://api.shunnet.top/pic/nuget.png" height="28"> VisualIdentity  

> 🚀 **基于 .NET 9 的多模型智能识别平台**  
> 高效 · 灵活 · 易部署  


## 🌟 项目简介  

在 **AI 应用落地** 的过程中，**模型管理** 与 **多任务识别** 一直是开发者的痛点。  
无论是 **检测、分类、分割、姿态估计、定向检测**，往往都需要同时部署多个模型，传统方案在 **效率** 和 **易用性** 上总会遇到瓶颈。  

**VisualIdentity** 正是为了解决这一系列问题而生。  
它结合了 **.NET 9** 的现代化能力、[YoloDotNet](https://github.com/NickSwardh/YoloDotNet) 的高性能推理、以及 **SQLite** 的轻量级管理，为开发者提供一个 **开箱即用** 的智能识别平台。  

✅ 多模型管理  
✅ 单机多任务识别  
✅ 跨平台部署  


## 🎯 应用场景  

- 🏭 **工业质检**：瑕疵检测、异物识别  
- 🛒 **零售分析**：顾客行为、货架检测  
- 🛡️ **智能安防**：异常行为、姿态识别  
- 🎓 **科研教育**：多模型实验平台  
- 🌐 **边缘计算**：轻量化部署到嵌入式或服务器  


## 📦 NuGet 安装  

```bash
dotnet add package Snet.Yolo.Server
```


## ⚙️ 功能特性  

### 🔹 多模型管理  
- 支持 **增 / 删 / 改 / 查**  
- 模型 **版本化 & 快速切换**  
- 一机多模型轻松维护  

### 🔹 单机多任务流畅运行  
- 支持 **检测 / OBB 定向检测 / 分类 / 分割 / 姿态估计**  
- 基于 **YoloDotNet 高速推理内核**  
- **零配置，一键运行**  

### 🔹 跨平台 & 部署友好  
- 支持 **Windows / Linux / Docker 部署**  
- 提供轻量化配置，适配 **边缘设备 & 服务器**  
- **开箱即用，降低开发门槛**  


## 📚 依赖组件  

### [Snet.DB](https://www.nuget.org/packages/Snet.DB)  
- 集成 **Dapper & SqlSugarCore**  
- 支持高性能 **SQL 映射与链式查询**  
- 自动建表，高效开发  
- 保持轻量同时，具备 **生产级性能**  

### [YoloDotNet](https://github.com/NickSwardh/YoloDotNet)  
- C# 生态下 **极快、功能齐全** 的 YOLO 推理库  
- 支持 **YOLOv5u - YOLOv12、YOLO World、YOLO-E**  
- 功能覆盖：**检测 / OBB / 分割 / 分类 / 姿态估计 / 跟踪**  


## 🧩 支持的版本  

```
YOLOv5u | YOLOv8 | YOLOv9 | YOLOv10 | YOLOv11 | YOLOv12 | YOLO-World | YOLO-E
```


## 🔬 支持的任务  

| 分类 (Classification) | 检测 (Detection) | OBB 定向检测 | 分割 (Segmentation) | 姿态估计 (Pose) |
|:---:|:---:|:---:|:---:|:---:|
| <img src="https://user-images.githubusercontent.com/35733515/297393507-c8539bff-0a71-48be-b316-f2611c3836a3.jpg" width=240> | <img src="https://user-images.githubusercontent.com/35733515/273405301-626b3c97-fdc6-47b8-bfaf-c3a7701721da.jpg" width=240> | <img src="https://github.com/NickSwardh/YoloDotNet/assets/35733515/d15c5b3e-18c7-4c2c-9a8d-1d03fb98dd3c" width=240> | <img src="https://github.com/NickSwardh/YoloDotNet/assets/35733515/3ae97613-46f7-46de-8c5d-e9240f1078e6" width=240> | <img src="https://github.com/NickSwardh/YoloDotNet/assets/35733515/b7abeaed-5c00-4462-bd19-c2b77fe86260" width=240> |
| <sub>[pexels.com](https://www.pexels.com/photo/hummingbird-drinking-nectar-from-blooming-flower-in-garden-5344570/)</sub> | <sub>[pexels.com](https://www.pexels.com/photo/men-s-brown-coat-842912/)</sub> | <sub>[pexels.com](https://www.pexels.com/photo/bird-s-eye-view-of-watercrafts-docked-on-harbor-8117665/)</sub> | <sub>[pexels.com](https://www.pexels.com/photo/man-riding-a-black-touring-motorcycle-903972/)</sub> | <sub>[pexels.com](https://www.pexels.com/photo/woman-doing-ballet-pose-2345293/)</sub> |  


## ⚡ 推理后端支持  

![ONNX Runtime](https://img.shields.io/badge/Backend-ONNX_Runtime-1f65dc?style=flat&logo=onnx)
![CPU](https://img.shields.io/badge/CPU-Supported-lightgrey?style=flat&logo=intel)
![CUDA](https://img.shields.io/badge/GPU-CUDA-76B900?style=flat&logo=nvidia)
![TensorRT](https://img.shields.io/badge/Inference-TensorRT-00BFFF?style=flat&logo=nvidia)  


## 🙏 致谢  

- 🌐 [Shunnet.top](https://shunnet.top)  
- 🔥 [Ultralytics](https://github.com/ultralytics/ultralytics)  
- ⚡ [YoloDotNet](https://github.com/NickSwardh/YoloDotNet)  


## 📜 许可证  

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)  

本项目基于 **MIT** 开源。  
请阅读 [LICENSE](LICENSE) 获取完整条款。  
⚠️ 软件按 “原样” 提供，作者不对使用后果承担责任。  


## 🌍 查阅  

👉 [点击跳转](https://shunnet.top/EaiUj)  
