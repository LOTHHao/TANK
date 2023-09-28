using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;            // Số vòng người chơi thắng để thắng trò chơi
        public float m_StartDelay = 3f;             // Thời gian chờ khi bắt đầu
        public float m_EndDelay = 3f;               // Thời gian chờ khi kết thúc 
        public CameraControl m_CameraControl;       // Tham chiếu đến CameraControl
        public Text m_MessageText;                  // Tham chiếu đến Text Thông báo
        public GameObject m_TankPrefab;             // Tham chiếu đến Tank người dùng sử dụng 
        public TankManager[] m_Tanks;               // Tham chiếu đến TankManager

        
        private int m_RoundNumber;                  // Số vòng chơi hiện tại
        private WaitForSeconds m_StartWait;         // Thời gian chờ khi bắt đầu 
        private WaitForSeconds m_EndWait;           // Thời gian chờ khi kết thúc
        private TankManager m_RoundWinner;          // Tham chiếu đến số vòng thắng của người chơi
        private TankManager m_GameWinner;           // Tham chiếu đến thông báo người chơi chiến thắng 


        private void Start()
        {
            //Tạo thời gian chờ
            m_StartWait = new WaitForSeconds (m_StartDelay);
            m_EndWait = new WaitForSeconds (m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            // Khi xe tank được khởi tạo camera sẽ theo tank và bắt đầu trò chơi
            StartCoroutine (GameLoop ());
        }


        private void SpawnAllTanks()
        {
            // Tạo Tank
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // Khởi tạo và cho vị trí của người chơi tương ứng
                m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }


        private void SetCameraTargets()
        {
            // Tạo 1 mảng các transform có cùng kích thước với tank
            Transform[] targets = new Transform[m_Tanks.Length];

            // Với mỗi thứ tự trong mảng
            for (int i = 0; i < targets.Length; i++)
            {
                // Đặt giá trị cho xe tank tương ứng
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // Mục tiêu camera theo dõi
            m_CameraControl.m_Targets = targets;
        }


        // Gọi khi mỗi lần bắt đầu game
        private IEnumerator GameLoop ()
        {
            // Bắt đầu với RoundStarting 
            yield return StartCoroutine (RoundStarting ());

            // Sau khi RoundStarting chạy xong thì đến RoundPlaying
            yield return StartCoroutine (RoundPlaying());

            // Say khi xong ván game thì chạy tiếp RoundEnding
            yield return StartCoroutine (RoundEnding());

            // Khi nào chạy xong hết dòng trên thì sẽ kiểm tra xem đã có ai thắng chưa
            if (m_GameWinner != null)
            {
                // Nếu có người chơi thắng thì sẽ load lại Scene
                SceneManager.LoadScene (0);
            }
            else
            {
                //Nếu có người chơi thắng thì load lại game
                StartCoroutine (GameLoop ());
            }
        }


        private IEnumerator RoundStarting ()
        {
            // Khi bắt đầu để chắc là tank không di chuyển
            ResetAllTanks ();
            DisableTankControl ();

            // Điều chỉnh Camera
            m_CameraControl.SetStartPositionAndSize ();

            // Tăng số vòng và hiển thị mỗi khi bắt đầu 
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Thời gian chờ trước khi bắt đầu 
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying ()
        {
            // Bật điều khiển tank khi bắt đầu
            EnableTankControl ();

            // Xóa Text khỏi màn hình
            m_MessageText.text = string.Empty;

            // Check điều kiện khi chỉ còn 1 tank
            while (!OneTankLeft())
            {
                yield return null;
            }
        }


        private IEnumerator RoundEnding ()
        {
            // Dừng tank di chuyển 
            DisableTankControl ();

            // Xóa người chiến thắng từ vòng trước 
            m_RoundWinner = null;

            // Gán người chiến thắng của vòng này
            m_RoundWinner = GetRoundWinner ();

            // Tăng điểm cho người chiến thắng 
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Kiểm tra xem có ai thắng chưa
            m_GameWinner = GetGameWinner ();

            // Nhận điểm số và hiển thị nó 
            string message = EndMessage ();
            m_MessageText.text = message;

            // Đợi 1 tí trước khi kết thúc
            yield return m_EndWait;
        }


        // Kiểm tra khi còn 1 tank 
        private bool OneTankLeft()
        {
            // Số tank ban đầu 
            int numTanksLeft = 0;

            // Mỗi tank trên sân sẽ + thêm
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // Nếu nó đang hiện hữu + thêm vào biến numTankLeft
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // Nếu còn lại 1 thì trả về true
            return numTanksLeft <= 1;
        }
        
        
        // Hàm này được gọi khi còn 1 tank hoạt động 
        private TankManager GetRoundWinner()
        {
            // Kiểm tra các tank đang hiện hữu
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // Kiểm tra tank còn lại
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // Nếu không có xe nào hoạt động thì đó trả về 1 trận hòa 
            return null;
        }


        // Kiểm tra tank chiến thắng
        private TankManager GetGameWinner()
        {
            // Kiểm tra các tank
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // Trả lại tank chiến thắng khi đủ số vòng thắng
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // Nếu không có tank nào chiến thăng thì trả về null
            return null;
        }


        // Nội dung hiển thị ở mỗi cuối vòng 
        private string EndMessage()
        {
            // Mặc định ban đầu nếu không có ai thắng thì sẽ là hòa
            string message = "DRAW!";

            // Hiển thị người chiến thắng
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            message += "\n\n\n\n";

            // Hiển thị số lần thắng của mỗi tank
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // Nếu có người chiến thắng thì hiển thị người đó
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // Đặt lại vị trí của tank
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }
}