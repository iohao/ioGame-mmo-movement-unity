using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    //是否正在移动
    protected bool isMoving = false;

    //移动目标点
    private Vector3 targetPosition;

    //移动速度
    public float speed = 1.2f;

    // 动画组件 
    private Animator animator;

    //描述
    public string desc = "";
    
    public string id = "";

    //移动到某处
    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);
    }

    //移动Update
    public void MoveUpdate()
    {
        if (isMoving == false)
        {
            return;
        }

        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos,
            targetPosition, speed * Time.deltaTime);
        transform.LookAt(targetPosition);
        if (Vector3.Distance(pos, targetPosition) < 0.05f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    // Use this for initialization
    protected void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected void Update()
    {
        MoveUpdate();
    }
}