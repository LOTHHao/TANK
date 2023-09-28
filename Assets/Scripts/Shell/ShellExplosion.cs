using UnityEngine;

namespace Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // Dùng để kiểm tra ảnh hưởng vụ nổ 
        public ParticleSystem m_ExplosionParticles;         // Tham chiếu đến hiệu ứng nổ
        public AudioSource m_ExplosionAudio;                // Tham chiếu đến âm thanh khi nổ
        public float m_MaxDamage = 100f;                    // Số sát thương tối đa
        public float m_ExplosionForce = 1000f;              // Lực nổ
        public float m_MaxLifeTime = 2f;                    // Thời gian tồn tại của đạn
        public float m_ExplosionRadius = 5f;                // Khoảng cách nổ


        private void Start ()
        {
            // Gán thời gian hủy đạn vào khi đã tạo
            Destroy (gameObject, m_MaxLifeTime);
        }


        private void OnTriggerEnter (Collider other)
        {
			// Thu thập các va chạm 
            Collider[] colliders = Physics.OverlapSphere (transform.position, m_ExplosionRadius, m_TankMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                // Tìm Rigidbody của vật đó
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();

                // Nếu không có rigibody skip qua
                if (!targetRigidbody)
                    continue;

                // Thêm lực nổ và bán kính nổ 
                targetRigidbody.AddExplosionForce (m_ExplosionForce, transform.position, m_ExplosionRadius);

                // Tìm component máu của tank
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();

                // Nếu không có skip qua
                if (!targetHealth)
                    continue;

                // Tính toán sát thương
                float damage = CalculateDamage (targetRigidbody.position);

                // Nhận sát thương
                targetHealth.TakeDamage (damage);
            }

            m_ExplosionParticles.transform.parent = null;

            // Chạy hiệu ứng nổ
            m_ExplosionParticles.Play();

            // Chạy âm thanh nổ
            m_ExplosionAudio.Play();

            // Hiệu ưng nổ chạy xong thì hủy 
            ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
            Destroy (m_ExplosionParticles.gameObject, mainModule.duration);

            // Hủy đạn
            Destroy (gameObject);
        }


        private float CalculateDamage (Vector3 targetPosition)
        {
            // Tính khoảng cách vụ nổ 
            Vector3 explosionToTarget = targetPosition - transform.position;

            // Tính khoảng cách đạn đến mục tiêu
            float explosionDistance = explosionToTarget.magnitude;

            // Tính tỷ lệ khoảng cách vụ nổ
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            // Tính lượng sát thương
            float damage = relativeDistance * m_MaxDamage;

            // Giới hạn sát thương tối thiểu là 0
            damage = Mathf.Max (0f, damage);

            return damage;
        }
    }
}