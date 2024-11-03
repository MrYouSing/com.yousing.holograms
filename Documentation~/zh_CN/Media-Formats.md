# 媒体文件格式
------------
基于Unity开发的播放器，所以支持的媒体格式有限。但满足大部分的使用场景，其中:  

图片格式:  
* .jpg  
* .jpeg  
* .png  

视频格式:  
* .mp4  
* .webm  

为了支持播放多种3D类型，采取了特殊的命名方式，通常为文件名+参数+后缀名。  
> 下面文件命名说明中，{}为参数边界，[]为可选参数。  
## 双目格式  
-----------
传统3D显示器支持的格式，如常见的3D电影视频。  
> 文件名[_a{宽高比}][\_{布局类型}].后缀名  

``例如: vid_bigbuckbunny_ou.mp4``  

布局类型如下:  
* sbs:左右布局(**默认**)  
* ou:上下布局  

## [深度格式](https://docs.lookingglassfactory.com/software-tools/looking-glass-studio/rgbd-photo-video)  
------------
深度媒体比普通媒体多了一个深度通道，可重建深度来还原3D场景。通常可以从下面方式获取:  
* 硬件:能采集深度的录制设备，如Kinect.iPhone等。  
* 软件:2D转深度的AI方案。  

### 单文件(支持图片和视频)  
> 文件名[_a{宽高比}][\_{布局类型}]_rgbd.后缀名  

``例如: Sailermoon-depth-map_rgbd.jpg``
### 双文件(仅支持图片)  
> 文件名[_a{宽高比}]_rgb.后缀名  
> 文件名[_a{宽高比}]_depth.后缀名  

``例如: Grand-Canyon-Depth-Map_rgb.png 和 Grand-Canyon-Depth-Map_depth.png``  
## [多视图格式](https://docs.lookingglassfactory.com/software-tools/looking-glass-studio/quilt-photo-video)
------------
LookingGlass类显示器的主流媒体格式。CompanionOne显示器对应的文件命名为:**文件名_qs8x5a-0.56.后缀名**。  
> 文件名_qs{列数}x{行数}a{宽高比}.后缀名  

``例如: WuKong_qs8x5a0.56.png``
## 模型格式  
------------
既然是Unity开发，当然可以查看模型。但是渲染压力巨大，请谨慎使用。  
* [Unity AssetBundle](https://docs.unity3d.com/Manual/AssetBundlesIntro.html)
> 后缀名:.ab和.unity
* [GLTF](https://github.com/KhronosGroup/glTF)
> 后缀名:.gltf和.glb
* 待定/未完成
> 后缀名:.obj和.fbx