using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
/*===============================================================*/
/**
* CSVを読み込み、CSV内の記号に対して設定したPrefabを生成するエディタ
* 2017年9月10日 Buravo
*/ 
public class CSVGameObjectGenerationEditor : EditorWindow 
{

    #region メンバ変数
    /*===============================================================*/
    /**
    * @brief CSV
    */
    private TextAsset m_csv;
    /**
    * @brief 生成するオブジェクトのプレハブ
    */
    private GameObject m_prefab;
    /**
    * @brief キーリスト
    */
    private List<string> m_key_list;
    /**
    * @brief オブジェクトリスト
    */
    private List<GameObject> m_prefab_list;
    /**
    * @brief キー名に紐付いたオブジェクトを管理するディクショナリ
    */
    private Dictionary<string, GameObject> m_object_dictionary;
    /**
    * @brief 生成するフィールドの個数
    */
    private int m_create_num = 0;
    /**
    * @brief スクロール座標
    */
    private Vector2 m_scroll_position;
    /*===============================================================*/
    #endregion 
 
    /*===============================================================*/
    /**
    * @brief 初期化処理
    */
    // メニューから呼び出せるエディタの項目を追加.
    [MenuItem("CustomMenu/CSVGameObjectGeneration")]
    static void Init () 
    {
        // 専用のウィンドウを表示.
        EditorWindow.GetWindow<CSVGameObjectGenerationEditor>(true, "CSVGameObjectGeneration");
    }
    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief ウィンドウ表示時に自動で呼ばれるメソッド
    */ 
    void OnEnable () 
    {
        // 初期化
        m_key_list = new List<string>(m_create_num);
        m_prefab_list = new List<GameObject>(m_create_num);
        m_object_dictionary = new Dictionary<string, GameObject>();
    }
    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief 選択内容の変更時に自動で呼ばれるメソッド
    */
    void OnSelectionChange () 
    {
        // 再描写
        Repaint();
    }
    /*===============================================================*/

    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief GUI表示処理
    */
    void OnGUI () 
    {
        try 
        {
            // CSV読み込みフィールドの表示.
            m_csv = EditorGUILayout.ObjectField("CSV", m_csv, typeof(Object), true) as TextAsset;

            // 座標の個数の入力フィールドの表示.
            m_create_num = EditorGUILayout.IntField("Number of Field:", m_create_num);
            
            // Labelの表示.
            GUILayout.Label("CSVKey, GameObject : ", EditorStyles.boldLabel);
            // 水平に配置するGUIグループの作成を開始.
            EditorGUILayout.BeginHorizontal();
            // レイアウトグループ内に全体の幅に対して均一となるスペースを生成し挿入する.
            GUILayout.FlexibleSpace();
            // 垂直に配置するGUIグループの作成を開始.
            EditorGUILayout.BeginVertical();

            
            // もしも０でなければ表示
            if (m_create_num >= 0)
            {
                // レイアウトイベント時に処理.
                if (Event.current.type == EventType.Layout)
                {
                    // 増減チェック.
                    CheckList();
                    // 入力フィールドの表示.
                    ListView();
                }
                else 
                {
                    // 座標の入力フィールドの表示.
                    ListView(); 
                }
            }
            // スペースで間隔をとる.
            EditorGUILayout.Space();
            // オブジェクトの生成.
            if (GUILayout.Button("Create", GUILayout.Width(150), GUILayout.Height(50)))
            {
                // CSV出力
                Debug.Log(m_csv.text);
                // CSVからオブジェクト生成
                Create(m_csv);
            }
            // 垂直に配置するGUIグループの作成を終了.
            EditorGUILayout.EndVertical();
            // レイアウトグループ内に全体の幅に対して均一となるスペースを生成し挿入する.
            GUILayout.FlexibleSpace();
            // 水平に配置するGUIグループの作成を終了.
            EditorGUILayout.EndHorizontal();
        } 
        catch (System.FormatException) 
        {
        }
    }
    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief インスペクタ更新処理
    */
    void OnInspectorUpdate ()
    {
        Repaint();
    }
    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief オブジェクト生成
    */
    void Create (TextAsset t_text)
    {
        // ディクショナリ化
        for (int i = 0; i < m_key_list.Count; i++)
        {
          m_object_dictionary.Add(m_key_list[i], m_prefab_list[i]);
        }
        // CSVの行数
        int height = 0;
        // CSVの列数
        int width = 0;
        // オブジェクトの連番
        int count = 0;
        StringReader reader = new StringReader(t_text.text);
        // 読み込みできる文字がなくなるまで繰り返す
        while(reader.Peek() >= 0) {
          // CSVを一行読み込み
          string line = reader.ReadLine();
          // 一行を一文字へパース
          foreach(string csvKey in line.Split(',')){
              // キーリスト内の文字列とCSV内の記号が一致したら処理
              if (m_key_list.Contains(csvKey) == true){
                  // オブジェクト生成
                  GameObject obj = Instantiate(m_object_dictionary[csvKey], new Vector3(width, height, 0), Quaternion.identity) as GameObject;
                  obj.name = m_object_dictionary[csvKey].name + count;
                  // 連番の増加
                  count++;
              }
              // 幅の増加
              width++;
          }
          // 行数の増加
          height++;
          // 幅の初期化
          width=0;
        }

    }
    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief 増減した個数をチェックし,リストを増減するメソッド
    */
    void CheckList ()
    {
        // キーの増減
        if (m_key_list.Count < m_create_num)
        {
            for (int i = 0; i < (m_create_num - m_key_list.Count); i++)
            {
                m_key_list.Add("");
            }
        }
        else if (m_key_list.Count > m_create_num)
        {
            for (int i = 0; i < (m_key_list.Count - m_create_num); i++)
            {
                m_key_list.RemoveAt(m_key_list.Count-1);
            }
        }

        // オブジェクトリストの増減
        if (m_prefab_list.Count < m_create_num)
        {
            for (int i = 0; i < (m_create_num - m_prefab_list.Count); i++)
            {
                m_prefab_list.Add(null);
            }
        }
        else if (m_prefab_list.Count > m_create_num)
        {
            for (int i = 0; i < (m_prefab_list.Count - m_create_num); i++)
            {
                m_prefab_list.RemoveAt(m_prefab_list.Count-1);
            }
        }
    }
    /*===============================================================*/

    /*===============================================================*/
    /**
    * @brief 一覧表示メソッド
    */
    void ListView ()
    {
        // スクロールビューの表示を開始.
        m_scroll_position = EditorGUILayout.BeginScrollView(m_scroll_position, GUILayout.Height(100));
        // 入力フィールド作成.
        for (int i = 0; i < m_key_list.Count; i++)
        {
            m_key_list[i] = EditorGUILayout.TextField("Key : ", m_key_list[i]);
            m_prefab_list[i] = EditorGUILayout.ObjectField("Prefab : ", m_prefab_list[i], typeof(GameObject), true) as GameObject;
        }
        // スクロールビューの表示を終了.
        EditorGUILayout.EndScrollView();
    }
    /*===============================================================*/

}
/*===============================================================*/