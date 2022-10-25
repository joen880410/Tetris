using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Shape : MonoBehaviour
{
    private Transform mPivot;
    private Controller mControllerInstance;
    private bool mIsPause;
    public float normalStepTime;
    private float mTimer;
    private float mInputTimer;

    private float mIsSpeedUp = 1;
    private bool mIsRocket;
    private bool mHasRocket;
    private const float moveTime = 0.2f;

    // Use this for initialization
    void Awake()
    {

        mPivot = transform.Find("Pivot");
        mControllerInstance = Controller.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (mIsPause)
        {
            return;
        }
        mTimer += Time.deltaTime;
        //input
        InputControl();

        if (mIsRocket)
        {
            Fall(5);
            if (!mHasRocket)
            {
                EventManager.Instance.Fire(UIEvent.CAMERA_SHAKE);
                mHasRocket = true;
            }
        }
        else
        {
            if (mTimer > normalStepTime / mIsSpeedUp)
            {
                Debug.Log("Fall");
                mTimer = 0;
                Fall();
            }

        }

    }
    public float Upgrade()
    {
        //升级
        normalStepTime /= 1.5f;
        return normalStepTime;
    }

    private void InputControl()
    {
        mInputTimer += Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow) && mInputTimer > moveTime)
        {
            StepLeft();
            mInputTimer = 0;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && mInputTimer > moveTime)
        {
            StepRight();
            mInputTimer = 0;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateShape();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SpeedUp();
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            SpeedDown();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rocket();
        }
    }

    public void Rocket()
    {
        if (mIsRocket)
        {
            return;
        }
        mIsRocket = true;
    }

    public void StepLeft()
    {
        lock (mPivot)
        {
            var newTransform = transform.position;
            newTransform.x -= 1;
            transform.position = newTransform;
            //如果不能转
            if (mControllerInstance.model.IsShapePositionValid(transform) == false)
            {
                newTransform.x += 1;
                transform.position = newTransform;
            }
            else
            {
                AudioManager.Instance.PlayControl();
            }
        }
    }

    public void StepRight()
    {
        lock (mPivot)
        {
            var newTransform = transform.position;
            newTransform.x += 1;
            transform.position = newTransform;
            //如果不能转
            if (mControllerInstance.model.IsShapePositionValid(transform) == false)
            {
                newTransform.x -= 1;
                transform.position = newTransform;
            }
            else
            {
                AudioManager.Instance.PlayControl();

            }
        }
    }

    public void SpeedUp()
    {
        mIsSpeedUp = 10;
    }
    public void SpeedDown()
    {
        mIsSpeedUp = 1;
    }
    public void RotateShape()
    {
        transform.RotateAround(mPivot.position, Vector3.forward, -90);
        //如果不能转
        if (mControllerInstance.model.IsShapePositionValid(transform) == false)
        {
            transform.RotateAround(mPivot.position, Vector3.forward, 90);
        }
        else
        {
            AudioManager.Instance.PlayControl();

        }
    }

    private void Fall(int step = 1)
    {
        while (true)
        {
            var position = transform.position;
            position.y -= step;
            transform.position = position;
            if (mControllerInstance.model.IsShapePositionValid(transform) == false)
            {
                position.y += step;
                transform.position = position;
                if (step == 1)
                {
                    mIsPause = true;
                    //储存当前数据>>检测是否需要消除行
                    mControllerInstance.model.PlaceShape(transform);
                    //新shape或结束
                    GameManager.Instance.ShapeFallDown();
                    break;
                }
                step = step - 1;
                continue;
            }
            AudioManager.Instance.PlayDrop();
            break;
        }
    }

    public void Resume()
    {
        mIsPause = false;
    }

    public void Pause()
    {
        mIsPause = true;
    }

    public void Init(Color color, float initSpeed)
    {

        //YOUNG 遍历
        foreach (Transform block in transform)
        {
            if (block.CompareTag("Block"))
            {
                block.GetComponent<SpriteRenderer>().color = color;
            }
        }
        normalStepTime = initSpeed;
    }

    public void SetSpeed(float speed)
    {
        normalStepTime = speed;
    }
}
