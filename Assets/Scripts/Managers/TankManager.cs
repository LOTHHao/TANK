using System;
using UnityEngine;

namespace Complete
{
    [Serializable]
    public class TankManager
    {
        public Color m_PlayerColor;                             // Màu của mỗi người chơi
        public Transform m_SpawnPoint;                          // Vị trí Spawn
        [HideInInspector] public int m_PlayerNumber;            // Số định danh của người chơi
        [HideInInspector] public string m_ColoredPlayerText;    // Màu của người chơi
        [HideInInspector] public GameObject m_Instance;         // Tham chiếu đến xe tank được tạo
        [HideInInspector] public int m_Wins;                    // Số trận thắng của người chơi
        

        private TankMovement m_Movement;                        // Tham chiếu class di chuyển
        private TankShooting m_Shooting;                        // Tham chiếu đến class Bắn
        private GameObject m_CanvasGameObject;                  


        public void Setup ()
        {
            // Tham chiếu đến các Component cần thiết 
            m_Movement = m_Instance.GetComponent<TankMovement> ();
            m_Shooting = m_Instance.GetComponent<TankShooting> ();
            m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas> ().gameObject;

            // Đặt các phím cho các người chơi tương ứng
            m_Movement.m_PlayerNumber = m_PlayerNumber;
            m_Shooting.m_PlayerNumber = m_PlayerNumber;

            // Tạo màu và tên người chơi 
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

            // Lưu trữ tất cả renderer của tank 
            MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();

            for (int i = 0; i < renderers.Length; i++)
            {
                // Đặt màu cho các tank tương ứng
                renderers[i].material.color = m_PlayerColor;
            }
        }


        // Dùng để tắt điều khiển 
        public void DisableControl ()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive (false);
        }


        // Dùng để bật điều khiển
        public void EnableControl ()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive (true);
        }


        // Reset về vị trí ban đầu
        public void Reset ()
        {
            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;

            m_Instance.SetActive (false);
            m_Instance.SetActive (true);
        }
    }
}