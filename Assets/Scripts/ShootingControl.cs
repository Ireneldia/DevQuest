using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingControl : MonoBehaviour
{
    [Header("Preset Fields")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Settings")]
    [SerializeField] private float rayDistance = 1000f;
    
    private int enemyLayerMask;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = GetComponentInChildren<Camera>();
        
        // Enemy Layer만 감지하도록 설정
        enemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // 마우스 위치를 월드 좌표 Ray로 변환
        Ray screenRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        Ray ray = new Ray(mainCamera.transform.position, screenRay.direction);
        RaycastHit hit;

        // Enemy Layer만 감지
        if (Physics.Raycast(ray, out hit, rayDistance, enemyLayerMask))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Object Pool에 반환
                if (EnemyPool.instance != null)
                {
                    EnemyPool.instance.ReturnEnemy(enemy.gameObject);
                }
                else
                {
                    Destroy(enemy.gameObject);
                }
            }
        }
    }
}