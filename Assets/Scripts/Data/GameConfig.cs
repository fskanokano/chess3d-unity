using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏资源引用配置 - ScriptableObject
/// 集中管理所有提取的素材引用，对接 GameManager/AudioManager/UIManager
/// 对应原版中通过 Inspector 拖拽赋值的 Prefab 和 AudioClip 引用
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Chess3D/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("=== 棋子 3D 模型预制体 ===")]
    [Tooltip("兵/卒 - bing1.glb (6.5MB 含骨骼层级)")]
    public GameObject bingPrefab;
    [Tooltip("车 - Che.glb (1.0MB)")]
    public GameObject chePrefab;
    [Tooltip("马 - Ma.glb (1.2MB)")]
    public GameObject maPrefab;
    [Tooltip("炮 - Pao.glb (656KB)")]
    public GameObject paoPrefab;
    [Tooltip("仕/士 - Shi.glb (1.4MB)")]
    public GameObject shiPrefab;
    [Tooltip("帅/将 - Shuai.glb (4.5MB)")]
    public GameObject shuaiPrefab;
    [Tooltip("象/相 - xiang.glb (142KB)")]
    public GameObject xiangPrefab;

    [Header("=== 棋子纹理 (红/黑两套) ===")]
    [Tooltip("炮 红 - pao_r.png")]
    public Texture2D paoRedTex;
    [Tooltip("炮 黑 - pao_b.png")]
    public Texture2D paoBlackTex;
    [Tooltip("象 红 - xiang_r.png")]
    public Texture2D xiangRedTex;
    [Tooltip("象 黑 - xiang_b.png")]
    public Texture2D xiangBlackTex;
    [Tooltip("士 红 - shid_r.png")]
    public Texture2D shiRedTex;
    [Tooltip("士 黑 - shid_b.png")]
    public Texture2D shiBlackTex;
    [Tooltip("马 红 - renma_r.png")]
    public Texture2D maRedTex;
    [Tooltip("马 黑 - renma_b.png")]
    public Texture2D maBlackTex;
    [Tooltip("兵 黑 - bing_b.png")]
    public Texture2D bingBlackTex;
    [Tooltip("卒 红 - zhu_D_r.png")]
    public Texture2D zhuRedTex;
    [Tooltip("棋子通用 - m_144_b.png")]
    public Texture2D chess通用Tex;
    [Tooltip("将/帅 - wang.png")]
    public Texture2D wangTex;

    [Header("=== 角色皮肤纹理 ===")]
    [Tooltip("中年武将 - N_中年武将_D.png (6.4MB)")]
    public Texture2D 中年武将Tex;
    [Tooltip("诸葛亮1 - ZhuGeiLiang_1_D.png")]
    public Texture2D 诸葛亮1Tex;
    [Tooltip("诸葛亮2 - ZhuGeiLiang_2_D.png")]
    public Texture2D 诸葛亮2Tex;
    [Tooltip("貂禅衣1 - 貂禅衣.png")]
    public Texture2D 貂禅衣1Tex;
    [Tooltip("貂禅衣2 - 貂禅衣2.png")]
    public Texture2D 貂禅衣2Tex;

    [Header("=== 棋盘 ===")]
    [Tooltip("棋盘纹理 - qipan.png (1024x1024)")]
    public Texture2D qipanTex;
    [Tooltip("棋盘 3D 模型 - OceanPlane.glb")]
    public GameObject boardPrefab;

    [Header("=== 音效 ===")]
    [Tooltip("背景音乐 - bg01.ogg")]
    public AudioClip bgmClip;
    [Tooltip("备选BGM - mybg01.ogg")]
    public AudioClip bgmAltClip;
    [Tooltip("走棋音效 - ChessMove3.ogg")]
    public AudioClip moveClip;
    [Tooltip("吃子音效 - EatChess.ogg")]
    public AudioClip eatClip;
    [Tooltip("将军音效 - JiangJun.ogg")]
    public AudioClip checkClip;
    [Tooltip("按钮点击 - Button.ogg")]
    public AudioClip clickClip;
    [Tooltip("攻击音效 - attack2.ogg")]
    public AudioClip attackClip;
    [Tooltip("胜利音效 - shengli.ogg")]
    public AudioClip winClip;
    [Tooltip("失败/败 - bai.ogg")]
    public AudioClip loseClip;
    [Tooltip("胜利音乐 - shengliyinyue.ogg")]
    public AudioClip winMusicClip;
    [Tooltip("开始攻击 - startatt.ogg")]
    public AudioClip startAttClip;
    [Tooltip("鼓声 - gu.ogg")]
    public AudioClip drumClip;

    [Header("=== 棋子语音 ===")]
    [Tooltip("车语音 - juese/ju.ogg")]
    public AudioClip cheVoice;
    [Tooltip("马语音 - juese/ma.ogg")]
    public AudioClip maVoice;
    [Tooltip("炮语音 - juese/pao.ogg")]
    public AudioClip paoVoice;
    [Tooltip("士语音 - juese/shi.ogg")]
    public AudioClip shiVoice;
    [Tooltip("帅语音 - juese/shuai.ogg")]
    public AudioClip shuaiVoice;
    [Tooltip("象语音 - juese/xiang.ogg")]
    public AudioClip xiangVoice;
    [Tooltip("卒语音 - juese/zhu.ogg")]
    public AudioClip bingVoice;

    [Header("=== UI 精灵 ===")]
    [Tooltip("棋子精灵图集 - Chess.png (2048x2048)")]
    public Texture2D chessAtlasTex;
    [Tooltip("背景 - Background.png")]
    public Sprite background;
    [Tooltip("按钮 - btn02.png")]
    public Sprite buttonSprite;

    [Header("=== 场景层级 ===")]
    [Tooltip("红方棋子父节点")]
    public GameObject redParent;
    [Tooltip("黑方棋子父节点")]
    public GameObject blackParent;

    /// <summary>
    /// 根据棋子 ID 获取对应纹理
    /// ID 编码: 个位=类型(1车2马3象4士5帅6士7象8马9车), 十位=阵营
    /// </summary>
    public Texture2D GetTextureByID(int chessID)
    {
        int absId = Mathf.Abs(chessID);
        int type = absId % 10;
        bool isRed = chessID < 0;

        switch (type)
        {
            case 1: case 9: // 车 - 使用 wang 纹理
                return wangTex;
            case 2: case 8: // 马
                return isRed ? maRedTex : maBlackTex;
            case 3: case 7: // 象/相
                return isRed ? xiangRedTex : xiangBlackTex;
            case 4: case 6: // 士/仕
                return isRed ? shiRedTex : shiBlackTex;
            case 5: // 帅/将
                return wangTex;
            case 0: // 炮
                return isRed ? paoRedTex : paoBlackTex;
            default:
                // 兵/卒 (22-26红兵, 42-46黑卒)
                if (absId >= 22 && absId <= 26)
                    return chess通用Tex; // 红兵用通用纹理
                if (absId >= 42 && absId <= 46)
                    return isRed ? zhuRedTex : bingBlackTex;
                return chess通用Tex;
        }
    }

    /// <summary>
    /// 根据棋子 ID 获取语音音效
    /// </summary>
    public AudioClip GetVoiceByID(int chessID)
    {
        int absId = Mathf.Abs(chessID);
        int type = absId % 10;

        switch (type)
        {
            case 1: case 9: return cheVoice;
            case 2: case 8: return maVoice;
            case 3: case 7: return xiangVoice;
            case 4: case 6: return shiVoice;
            case 5: return shuaiVoice;
            case 0: return paoVoice;
            default: return bingVoice;
        }
    }
}
