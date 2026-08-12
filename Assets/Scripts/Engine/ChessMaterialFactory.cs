using UnityEngine;

/// <summary>
/// 棋子材质工厂 - 运行时创建红/黑两套材质
/// 对应原版中 Character.ChangeTextureFunc() 的材质切换逻辑
/// 
/// 原版材质系统:
/// - 每个棋子有多个子材质(materials数组)
/// - ChangeTextureFunc("red"/"blue") 激活对应材质
/// - 红方 = 负ID, 黑方 = 正ID
/// </summary>
public static class ChessMaterialFactory
{
    /// <summary>
    /// 为棋子创建红/黑双材质
    /// </summary>
    public static void SetupMaterials(GameObject piece, int chessID, GameConfig config)
    {
        if (piece == null || config == null) return;

        var renderer = piece.GetComponent<Renderer>();
        if (renderer == null) renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        // 获取对应纹理
        Texture2D tex = config.GetTextureByID(chessID);
        if (tex == null) return;

        // 创建红色材质
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.name = "red";
        redMat.mainTexture = tex;
        redMat.color = new Color(1f, 0.2f, 0.2f); // 红色色调
        redMat.SetFloat("_Glossiness", 0.3f);

        // 创建蓝色材质
        Material blueMat = new Material(Shader.Find("Standard"));
        blueMat.name = "blue";
        blueMat.mainTexture = tex;
        blueMat.color = new Color(0.2f, 0.2f, 0.2f); // 黑色色调
        blueMat.SetFloat("_Glossiness", 0.3f);

        // 设置材质数组
        renderer.materials = new Material[] { redMat, blueMat };

        // 激活对应颜色
        bool isBlack = chessID > 0;
        Character character = piece.GetComponent<Character>();
        if (character != null)
        {
            character.materials = new GameObject[2];
            // 创建材质切换辅助对象
            var matObj1 = new GameObject("red");
            matObj1.transform.SetParent(piece.transform);
            matObj1.SetActive(false);
            var matObj2 = new GameObject("blue");
            matObj2.transform.SetParent(piece.transform);
            matObj2.SetActive(false);

            character.materials[0] = matObj1;
            character.materials[1] = matObj2;

            character.ChangeTextureFunc(isBlack ? "blue" : "red", isBlack);
        }
    }

    /// <summary>
    /// 为场景中所有棋子批量设置材质
    /// </summary>
    public static void SetupAllPieces(GameManager gm, GameConfig config)
    {
        if (gm == null || config == null) return;

        // 遍历棋盘创建所有棋子
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int id = gm.chessBoard[i, j];
                if (id == 0) continue;

                // 获取对应预制体
                GameObject prefab = GetPrefabByID(id, gm);
                if (prefab == null) continue;

                Vector3 pos = gm.boardGrid[i, j].transform.position;
                pos.y += 2f;

                GameObject piece = Object.Instantiate(prefab, pos, Quaternion.identity);
                SetupMaterials(piece, id, config);

                // 设置父节点
                Transform parent = id < 0 ? gm.redParent?.transform : gm.blackParent?.transform;
                if (parent != null)
                    piece.transform.SetParent(parent);
            }
        }
    }

    /// <summary>
    /// 根据 ID 获取预制体 (与 GameManager.GetPrefabByID 一致)
    /// </summary>
    private static GameObject GetPrefabByID(int id, GameManager gm)
    {
        int absId = Mathf.Abs(id);
        int type = absId % 10;

        switch (type)
        {
            case 1: case 9: return gm.chePrefab;
            case 2: case 8: return gm.maPrefab;
            case 3: case 7: return gm.xiangPrefab;
            case 4: case 6: return gm.shiPrefab;
            case 5: return gm.shuaiPrefab;
            case 0: return gm.paoPrefab;
            default:
                if (absId >= 22 && absId <= 46) return gm.bingPrefab;
                return gm.paoPrefab;
        }
    }
}
