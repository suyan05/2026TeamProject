using UnityEngine;

public class MonsterContactDamage : MonoBehaviour
{
    [Header("데미지 설정")]
    public float contactDamage = 10f;       // 몬스터가 플레이어에게 줄 기본 데미지
    public float damageCooldown = 1.0f;     // 연속 데미지 방지용 쿨타임 

    private float lastDamageTime;

   
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDealDamage(collision.gameObject);
        }
    }

 
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryDealDamage(other.gameObject);
        }
    }

    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDealDamage(collision.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TryDealDamage(other.gameObject);
        }
    }

   
    private void TryDealDamage(GameObject playerObj)
    {
        
        if (Time.time < lastDamageTime + damageCooldown) return;

       
        var movement3D = playerObj.GetComponent<PlayerMovement_3D>();
        if (movement3D != null)
        {
            lastDamageTime = Time.time;
            movement3D.GetDamage(contactDamage, transform);
            Debug.Log($"[몬스터 공격] 플레이어({playerObj.name})에게 {contactDamage} 만큼의 물리 피해를 입혔습니다!");
            return;
        }

        playerObj.SendMessage("GetDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
    }
}