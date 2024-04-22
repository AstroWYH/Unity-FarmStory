using UnityEngine;

// note: �浵��6���ṩΨһ�ʲ�ID���༭��ģʽ��Ҳִ��
[ExecuteAlways]
public class DataGUID : MonoBehaviour
{
    public string guid;

    private void Awake()
    {
        if (guid == string.Empty)
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}