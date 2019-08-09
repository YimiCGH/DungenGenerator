# DungenGenerator
 
 
本工程参考自：
https://github.com/DMeville/Unity3d-Dungeon-Generator.git

以及原作者在论坛上对于算法的大概介绍
感谢原作者的无私分享
https://www.reddit.com/r/Unity3D/comments/3dt2in/procedural_dungeon_generator_im_working_on/

---
本人对其算法进行了改造和整理，让它适应自己的需求

另外当前算法也有一些局限的地方
* 例如不能旋转房间来适配接口
* 单接口的房间如Boss房间，死胡同，已经做了例外处理，可以旋转适配
* 存在一种情况导致生成失败（即两个房间的接口相对着，又刚好空一个单位体素的时候）
 如 ▙ ▟，因为没有可以放到中间并且有两个接口来同时连接两边的房间（或通道），所以一直在哪寻找。如果遇到这种情况，可以简单判断失败多少次就重新选择随机种子来再次进行生成
