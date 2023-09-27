using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : MonoBehaviour
    {
        public int m_PlayerNumber = 1;              // Định danh player
        public Rigidbody m_Shell;                   // Prefab đạn
        public Transform m_FireTransform;           // Vị trí bắn đạn
        public Slider m_AimSlider;                  // Hiển thị tầm bắn 
        public AudioSource m_ShootingAudio;         // Truy cứu tới dữ liệu âm thanh 
        public AudioClip m_ChargingClip;            // Âm thanh khi gồng bắn
        public AudioClip m_FireClip;                // Âm thanh mỗi lần bắn 
        public float m_MinLaunchForce = 15f;        // Lực gồng bắn tối thiểu
        public float m_MaxLaunchForce = 30f;        // Lực gồng bắn tối đa
        public float m_MaxChargeTime = 0.75f;       // Thời gian gồng tối đa


        private string m_FireButton;                // Nút bắn
        private float m_CurrentLaunchForce;         // Lực bắn hiện tại
        private float m_ChargeSpeed;                // Tốc độ gồng
        private bool m_Fired;                       // Check đạn đã được bắn hay chưa


        private void OnEnable()
        {
            // Khi xe tank được bật chỉnh lực và thanh canh chỉnh về min
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;
        }


        private void Start ()
        {
            // Nút bắn trên mỗi Player
            m_FireButton = "Fire" + m_PlayerNumber;

            // Tốc độ lực phóng tăng lên và phạm vi lực có theo thời gian phóng tối đa.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }


        private void Update ()
        {
            // Thanh canh chỉnh có giá trị tối thiểu 
            m_AimSlider.value = m_MinLaunchForce;

            // Nếu giá trị gồng đã max và chưa bắn 
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // Xử dụng giá trị gồng lớn nhất 
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire ();
            }
            // Khi nút bắn được nhấn
            else if (Input.GetButtonDown (m_FireButton))
            {
                // khởi tạo lại bool bắn và lực bắn
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // Chuyển âm thanh bắn sang âm thanh gồng và chạy nó
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play ();
            }
            // Nếu nút bắn dược giữ và chưa bắn 
            else if (Input.GetButton (m_FireButton) && !m_Fired)
            {
                // Tăng lực bắn và thanh canh chỉnh 
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Nếu nút bắn dc thả và chưa bắn
            else if (Input.GetButtonUp (m_FireButton) && !m_Fired)
            {
                Fire ();
            }
        }


        private void Fire ()
        {
            // Để chắc chắn là bắn chỉ được gọi 1 lần
            m_Fired = true;

            // Tạo đạn và tham chiếu nó đến rigibody
            Rigidbody shellInstance = Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            // Đặt vận tốc bằng lực bắn về phía trước
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward; 

            // Đổi âm thanh bắn và chạy nó
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play ();

            // Đặt lại lực bắn 
            m_CurrentLaunchForce = m_MinLaunchForce;
        }
    }
}