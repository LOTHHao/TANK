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


        // Gọi nó khi mỗi lần bắt đầu game
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
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks ();
            DisableTankControl ();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize ();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying ()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl ();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }


        private IEnumerator RoundEnding ()
        {
            // Stop tanks from moving.
            DisableTankControl ();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner ();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner ();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage ();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end.
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if they are active, increment the counter.
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }
        
        
        // This function is to find out if there is a winner of the round.
        // This function is called with the assumption that 1 or fewer tanks are currently active.
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
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