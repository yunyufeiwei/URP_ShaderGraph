using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class RefDepCollecter
{
    //private const string CollectRootPath = "Assets/_LaoWang/RefDepTest";
    private static string[] CollectRootPath = { "Assets" /*, "Packages"*/};
    private const string CachePathDir = "Packages/com.gebilaowxj_outlook.snakeassettool/Res/Cache/";
    private const string RefDepCacheFilePath = CachePathDir + "RefDep.json";

    private static RefDepCollecter _ins;

    private bool _isRunning;
    private bool _haveCollected;

    private int _processCount;

    private string[] _allGuid;
    private AllAssetInfoData _allAssetInfo;

    public static RefDepCollecter Ins
    {
        get
        {
            if (_ins == null)
                _ins = new RefDepCollecter();
            return _ins;
        }
    }

    private RefDepCollecter()
    {
        _isRunning = false;
        _haveCollected = false;
        _processCount = Environment.ProcessorCount;
        GetLastAssetInfoFromFile();
    }


    private void GetLastAssetInfoFromFile()
    {
        if (File.Exists(RefDepCacheFilePath))
        {
            using (StreamReader sr = new StreamReader(File.Open(RefDepCacheFilePath, FileMode.Open)))
            {
                _allAssetInfo = JsonUtility.FromJson<AllAssetInfoData>(sr.ReadToEnd());
                _allAssetInfo.UpdateBaseInfo();
                int infoCount = _allAssetInfo.AllAssetInfo.Count;
                if (infoCount == 0)
                {
                    _haveCollected = false;
                }
                else
                {
                    _allAssetInfo.AllGuid.Capacity = infoCount;
                    foreach (RefDepAssetInfo info in _allAssetInfo.AllAssetInfo)
                    {
                        _allAssetInfo.AllGuid.Add(info.Guid);
                    }
                    _haveCollected = true;
                }
            }
        }
        else
        {
            _allAssetInfo = new AllAssetInfoData();
        }
    }

    public void ReCollectInfo()
    {
        if (_isRunning)
            return;
        _isRunning = true;
        _allAssetInfo.AllAssetInfo.Clear();
        _allAssetInfo.AllGuid.Clear();
        _allGuid = AssetDatabase.FindAssets("*", CollectRootPath);
        int guidCount = _allGuid.Length;
        if (guidCount == 0)
        {
            _isRunning = false;
            return;
        }

        #region ����Ż�
        //int batchSize = Mathf.CeilToInt(guidCount * 1.0f / _processCount);
        //List<Task> tsList = new List<Task>();
        //long starTime = DateTime.Now.Ticks;
        //for (int i = 0; i < _processCount; i++)
        //{
        //    int startIndex = i * batchSize;
        //    if (startIndex >= guidCount)
        //        break;
        //    int endIndex = startIndex + batchSize;
        //    endIndex = Mathf.Min(endIndex, guidCount);
        //    Debug.Log(startIndex.ToString() + "  " + endIndex.ToString());
        //    tsList.Add(Task.Run(() =>
        //    {
        //        SingleCoreCollectDep(startIndex, endIndex);
        //    }));
        //}
        //Task.WhenAll(tsList).ContinueWith(t =>
        //{
        //    long endTime = DateTime.Now.Ticks;
        //    TimeSpan ts = TimeSpan.FromTicks(endTime - starTime);
        //    Debug.Log("��ʱ: " + ts.TotalMilliseconds.ToString());
        //    Debug.Log(_allAssetInfo.Count.ToString());
        //    _isRunning = false;
        //});
        #endregion

        //long startTick = DateTime.Now.Ticks;
        //foreach (string guid in _allGuid)
        //{
        //    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        //    //��ʼ�ʲ�����
        //    RefDepAssetInfo assetInfo = GetAssetInfoByGuid(guid);
        //    if (assetInfo == null)
        //    {
        //        assetInfo = new RefDepAssetInfo();
        //        assetInfo.Guid = guid;
        //        assetInfo.AssetPath = assetPath;
        //        _allAssetInfo.AllAssetInfo.Add(assetInfo);
        //    }
        //    //Debug.Log(AssetDatabase.GetMainAssetTypeAtPath(assetPath) + "  " + assetPath);
        //    string[] depPaths = AssetDatabase.GetDependencies(assetPath, false);
        //    foreach (string depPath in depPaths)
        //    {
        //        string depGuid = AssetDatabase.AssetPathToGUID(depPath);
        //        if (depGuid == guid)
        //            continue;
        //        RefDepAssetInfo depInfo = GetAssetInfoByGuid(depGuid);
        //        if (depInfo == null)
        //        {
        //            depInfo = new RefDepAssetInfo();
        //            depInfo.Guid = depGuid;
        //            depInfo.AssetPath = depPath;
        //            _allAssetInfo.AllAssetInfo.Add(depInfo);
        //        }
        //        depInfo.AddRefGuid(guid);
        //        assetInfo.AddDepGuid(depGuid);
        //    }
        //}
        //long endTick = DateTime.Now.Ticks;
        //Debug.Log(TimeSpan.FromTicks(endTick-startTick).TotalMilliseconds.ToString());
        //Debug.Log(_allAssetInfo.AllAssetInfo.Count.ToString());

        #region ���������ȹ���������Ϣ���ٹ���������Ϣ�������˺ཻܶ���ѯ��Ч�ʷ�������ߡ�ͬʱ�ǳ����ڲ��л�
        ////long startTick = DateTime.Now.Ticks;
        //foreach (string guid in _allGuid)
        //{
        //    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        //    //��ʼ�ʲ�����
        //    RefDepAssetInfo assetInfo = new RefDepAssetInfo();
        //    assetInfo.Guid = guid;
        //    assetInfo.AssetPath = assetPath;
        //    _allAssetInfo.AllAssetInfo.Add(assetInfo);
        //    //Debug.Log(AssetDatabase.GetMainAssetTypeAtPath(assetPath) + "  " + assetPath);
        //    string[] depPaths = AssetDatabase.GetDependencies(assetPath, false);
        //    foreach (string depPath in depPaths)
        //    {
        //        string depGuid = AssetDatabase.AssetPathToGUID(depPath);
        //        if (depGuid == guid)
        //            continue;
        //        assetInfo.AddDepGuid(depGuid);
        //    }
        //}
        ////long endTick = DateTime.Now.Ticks;
        ////Debug.Log(TimeSpan.FromTicks(endTick - startTick).TotalMilliseconds.ToString());
        ////startTick = DateTime.Now.Ticks;
        //for (int i = 0; i < guidCount; i++)
        //{
        //    RefDepAssetInfo selfInfo = _allAssetInfo.AllAssetInfo[i];
        //    string selfGuid = selfInfo.Guid;
        //    foreach (string depGuid in selfInfo.DepList)
        //    {
        //        RefDepAssetInfo depInfo = GetAssetInfoByGuid(depGuid);
        //        if (depInfo == null)
        //        {
        //            depInfo = new RefDepAssetInfo();
        //            depInfo.Guid = depGuid;
        //            depInfo.AssetPath = AssetDatabase.GUIDToAssetPath(depGuid);
        //            _allAssetInfo.AllAssetInfo.Add(depInfo);
        //        }
        //        depInfo.AddRefGuid(selfGuid);
        //    }
        //}
        ////endTick = DateTime.Now.Ticks;
        ////Debug.Log(TimeSpan.FromTicks(endTick - startTick).TotalMilliseconds.ToString());
        ////Debug.Log(_allAssetInfo.AllAssetInfo.Count.ToString());

        long startTick = DateTime.Now.Ticks;
        GenDepInfo();
        //long endTick = DateTime.Now.Ticks;
        //Debug.Log("������������: " + TimeSpan.FromTicks(endTick - startTick).TotalSeconds.ToString());
        //startTick = DateTime.Now.Ticks;
        GenRefInfo();
        //endTick = DateTime.Now.Ticks;
        //Debug.Log("�������û���: " + TimeSpan.FromTicks(endTick - startTick).TotalSeconds.ToString());
        long endTick = DateTime.Now.Ticks;
        Debug.Log("��������: " + TimeSpan.FromTicks(endTick - startTick).TotalSeconds.ToString());
        #endregion

        //��¼������ļ���
        WriteResultToJsonCache();
       _isRunning = false;
    }

    private void WriteResultToJsonCache()
    {
        int infoCount = _allAssetInfo.AllGuid.Count;
        if (infoCount == 0)
        {
            _haveCollected = false;
            AssetDatabase.Refresh();
            return;
        }
        string jsonContent = JsonUtility.ToJson(_allAssetInfo);
        if (File.Exists(RefDepCacheFilePath))
        {
            File.WriteAllText(RefDepCacheFilePath, string.Empty);
        }
        using (StreamWriter sw = new StreamWriter(File.Open(RefDepCacheFilePath, FileMode.OpenOrCreate)))
        {
            sw.Write(jsonContent);
        }
        _haveCollected = true;
        AssetDatabase.Refresh();
    }

    private void GenDepInfo()
    {
        foreach (string guid in _allGuid)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            //��ʼ�ʲ�����
            RefDepAssetInfo assetInfo = new RefDepAssetInfo();
            assetInfo.Guid = guid;
            assetInfo.AssetPath = assetPath;
            _allAssetInfo.AllAssetInfo.Add(assetInfo);
            //Debug.Log(AssetDatabase.GetMainAssetTypeAtPath(assetPath) + "  " + assetPath);
            string[] depPaths = AssetDatabase.GetDependencies(assetPath, false);
            foreach (string depPath in depPaths)
            {
                string depGuid = AssetDatabase.AssetPathToGUID(depPath);
                if (depGuid == guid)
                    continue;
                assetInfo.AddDepGuid(depGuid);
            }
        }
        _allAssetInfo.AllGuid.AddRange(_allGuid);
    }

    private void GenRefInfo()
    {
        int guidCount = _allGuid.Length;
        for (int i = 0; i < guidCount; i++)
        {
            RefDepAssetInfo selfInfo = _allAssetInfo.AllAssetInfo[i];
            string selfGuid = selfInfo.Guid;
            foreach (string depGuid in selfInfo.DepList)
            {
                RefDepAssetInfo depInfo = GetAssetInfoByGuid(depGuid);
                if (depInfo == null)
                {
                    depInfo = new RefDepAssetInfo();
                    depInfo.Guid = depGuid;
                    depInfo.AssetPath = AssetDatabase.GUIDToAssetPath(depGuid);
                    _allAssetInfo.AllAssetInfo.Add(depInfo);
                    _allAssetInfo.AllGuid.Add(depGuid);
                }
                depInfo.AddRefGuid(selfGuid);
            }
        }
    }

    public RefDepAssetInfo GetAssetInfoByGuid(string guid)
    {
        if (!_haveCollected)
        {
            ReCollectInfo();
        }
        int guidCount = _allAssetInfo.AllGuid.Count;
        for (int i = 0; i < guidCount; i++)
        {
            if (_allAssetInfo.AllGuid[i] == guid)
                return _allAssetInfo.AllAssetInfo[i];
        }
        // �������ʲ��Ƿ�����������������������������
        return null;
    }

    public int GetIndexByGuid(string guid)
    {
        int guidCount = _allAssetInfo.AllGuid.Count;
        for (int i = 0; i < guidCount; i++)
        {
            if (_allAssetInfo.AllGuid[i] == guid)
                return i;
        }
        return -1;
    }

    //Unity�Դ�Json�޷�ֱ�����л�List���ʶ�������ת��
    [Serializable]
    private class AllAssetInfoData
    {
        // ��Ϊ���ٽṹ��ʹ����1.6W�ʲ���������ϵʱ������ܴ�15.2s => 13.5s����Ҫ������GetAssetInfoByGuid����ִ���ٶ�
        // ���濼����hashtable��һ������
        [NonSerialized]
        public List<string> AllGuid = new List<string>();
        public List<RefDepAssetInfo> AllAssetInfo = new List<RefDepAssetInfo>();

        public void UpdateBaseInfo()
        {
            foreach (RefDepAssetInfo info in AllAssetInfo)
            {
                if (info.Guid == string.Empty)
                    continue;
                info.AssetPath = AssetDatabase.GUIDToAssetPath(info.Guid);
            }
        }
    }

    public void ReplaceRef(RefDepAssetInfo oldInfo, RefDepAssetInfo newInfo)
    {
        if(oldInfo == null || newInfo == null) return;
        // �ҵ������ʲ�, oldInfo.RefListȫ����ӵ�newInfo.RefList
        // oldInfo.RefList�������ʲ���DepList���£�ɾ��oldInfo������newInfo
        foreach (string refGuid in oldInfo.RefList)
        {
            RefDepAssetInfo refInfo = GetAssetInfoByGuid(refGuid);
            if (refInfo == null)
            {// todo:�쳣��Ϣ
                continue;
            }
            if (!newInfo.RefList.Contains(refGuid))
            {// �Ѿ����ã�����Ч������Ϣ
                newInfo.RefList.Add(refGuid);
                refInfo.DepList.Remove(oldInfo.Guid);
                refInfo.DepList.Add(newInfo.Guid);
            }
        }
        oldInfo.RefList.Clear();
        //������Ҫˢ�½���
        WriteResultToJsonCache();
    }

    public void RemoveAsset(Object asset)
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        int index = GetIndexByGuid(guid);
        if (index < 0)
            return;
        RefDepAssetInfo info = _allAssetInfo.AllAssetInfo[index];
        foreach (string refGuid in info.RefList)
        {
            RefDepAssetInfo refInfo = GetAssetInfoByGuid(refGuid);
            refInfo.DepList.Remove(guid);
        }
        _allAssetInfo.AllGuid.RemoveAt(index);
        _allAssetInfo.AllAssetInfo.RemoveAt(index);
        WriteResultToJsonCache();
    }

    public void AddAsset(string guid)
    {
        
    }
}