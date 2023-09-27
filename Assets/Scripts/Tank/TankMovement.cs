using UnityEngine;

namespace Complete
{
    public class TankMovement : MonoBehaviour
    {
        public int m_PlayerNumber = 1;              // Định danh người chơi 
        public float m_Speed = 12f;                 // Vận tốc
        public float m_TurnSpeed = 180f;            // Tốc độ cua
        public AudioSource m_MovementAudio;         // Tham chiếu tới nguồn âm thanh di chuyển
        public AudioClip m_EngineIdling;            // Âm thanh khi tank đứng tại chỗ 
        public AudioClip m_EngineDriving;           // Âm thanh khi tank chạy
		public float m_PitchRange = 0.2f;           // Mức độ ồn của động cơ

        private string m_MovementAxisName;          // Nút di chuyển lên xuống
        private string m_TurnAxisName;              // Nút bo cua
        private Rigidbody m_Rigidbody;              // Tham chiếu đến xe tank
        private float m_MovementInputValue;         // Giá trị di chuyển lên xuống
        private float m_TurnInputValue;             // Giá trị bo cua
        private float m_OriginalPitch;              // Mức độ âm thanh khi bắt đầu
        private ParticleSystem[] m_particleSystems; // Tham chiếu đến hệ thống khói khi chạy

        private void Awake ()
        {
            m_Rigidbody = GetComponent<Rigidbody> ();
        }


        private void OnEnable ()
        {
            // Khi tank khởi tạo chắc chắn là nó đứng im
            m_Rigidbody.isKinematic = false;

            // Đặt lại các giá trị di chuyển 
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;

            // Hệ thống khói của tank khi chạy 
            m_particleSystems = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Play();
            }
        }


        private void OnDisable ()
        {
            // Khi xe tank tắt chuyển qua Kinematic để đứng im
            m_Rigidbody.isKinematic = true;

            // Tắt và reset hệ thống khói khi chạy
            for(int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Stop();
            }
        }


        private void Start ()
        {
            // Nút bấm theo từng người chơi
            m_MovementAxisName = "Vertical" + m_PlayerNumber;
            m_TurnAxisName = "Horizontal" + m_PlayerNumber;

            // Lưu trữ âm thanh lúc đầu 
            m_OriginalPitch = m_MovementAudio.pitch;
        }


        private void Update ()
        {
            // lưu trữ nút nhấn của player 
            m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
            m_TurnInputValue = Input.GetAxis (m_TurnAxisName);

            EngineAudio ();
        }


        private void EngineAudio ()
        {
            // Nếu không nhấn gì Tank đứng im
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                // Nguồn âm thanh di chuyển = âm thanh đang di chuyển 
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else
            {
                // Nếu tank đang di chuyển và âm thanh đứng im đang phát 
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // đổi âm thanh và chạy 
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }


        private void FixedUpdate ()
        {
            Move ();
            Turn ();
        }


        private void Move ()
        {
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }


        private void Turn ()
        {
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);
            m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
        }
    }
}