using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Generic;
using Microsoft.DirectX.Direct3D.CustomVertex;


namespace CUnit
{
  public abstract class Animation
  {
    public Animation() {}

    protected bool m_started = false;
    protected bool m_finished = false;
    protected float m_time = 0.0f;

    protected virtual void Init() {}

    public virtual void Update(float elapsedTime)
    {
      if (!m_started)
      {
        Init();
        m_started = true;
      }

      m_time += elapsedTime;
    }

    public bool IsFinished { get { return m_finished; } }
  }

  public abstract class ObjAnimation : Animation
  {
    public ObjAnimation(TextObj textObj) 
    {
      m_obj = textObj;
    }

    protected TextObj m_obj;
  }

  public class AnimationSet : Animation
  {
    public AnimationSet(Animation[] anims)
    {
      if (anims != null)
      {
        m_anims.AddRange(anims);
      }
    }

    protected List<Animation> m_anims = new List<Animation>();
    protected List<Animation> m_removeList = new List<Animation>();

    public override void Update(float elapsedTime)
    {
      base.Update(elapsedTime);

      m_removeList.Clear();
      foreach (Animation a in m_anims)
      {
        a.Update(elapsedTime);
        if (a.IsFinished)
        {
          m_removeList.Add(a);
        }
      }

      foreach (Animation a in m_removeList)
      {
        m_anims.Remove(a);
      }

      if (m_anims.Count == 0)
      {
        m_finished = true;
      }
    }

    public void AddAnimation(Animation anim)
    {
      m_anims.Add(anim);
      m_finished = false;
    }

    public void RemoveAnimation(Animation anim)
    {
      m_anims.Remove(anim);
      if (m_anims.Count == 0)
      {
        m_finished = true;
      }
    }

    public void StopAllAnimations()
    {
      m_anims.Clear();
      m_finished = true;
    }
  }

  public class AnimationSetQuickFinish : AnimationSet
  {
    public AnimationSetQuickFinish(Animation[] anims)
      : base(anims)
    {
    }

    public override void Update(float elapsedTime)
    {
      base.Update(elapsedTime);

      if (m_removeList.Count > 0)
      {
        m_finished = true;
      }
    }
  }

  public class AnimationSetOrdered : AnimationSet
  {
    public AnimationSetOrdered(Animation[] anims)
      : base(anims)
    {
    }

    public override void Update(float elapsedTime)
    {
      if (m_anims.Count > 0)
      {
        m_anims[0].Update(elapsedTime);
        if (m_anims[0].IsFinished)
        {
          m_anims.RemoveAt(0);
        }
      }

      if (m_anims.Count == 0)
      {
        m_finished = true;
      }
    }
  }

  public class ZoomAnim : ObjAnimation
  {
    public ZoomAnim(TextObj textObj, Vector3 dest, float duration, float accel)
      : base(textObj)
    {
      m_dest = dest;
      m_accel = accel;
      m_duration = duration;
    }

    protected float m_accel;
    protected Vector3 m_dest;
    protected Vector3 m_dir;
    protected float m_duration;
    protected float m_distance;

    protected override void Init()
    {
      Vector3 start = m_obj.CurrentTransform.Position;
      m_distance = (m_dest - start).Length();
      m_dir = m_dest - start;
      m_dir.Normalize();
    }

    public override void Update(float elapsedTime)
    {
      base.Update(elapsedTime);

      float fractionTravelled = 0f;
      if (m_duration > 0f)
      {
        // zoom in
        fractionTravelled = (float)Math.Pow(Math.Min(1.0f, m_time / m_duration), m_accel);
      }
      else
      {
        fractionTravelled = 1.0f;
      }

      float distanceTravelled = fractionTravelled * m_distance;
      m_obj.CurrentTransform.Position = m_dest + ((distanceTravelled - m_distance) * m_dir);

      if (m_time >= m_duration)
      {
        m_finished = true;
      }
    }
  }
  
  public class SpinAnimation : ObjAnimation
  {
    public SpinAnimation(TextObj textObj, float startSpeed, float endSpeed, float duration, bool waitForAlign) 
      : base(textObj)
    {
      m_startSpeed = startSpeed;
      m_endSpeed = endSpeed;
      m_duration = duration;
      m_waitForAlign = waitForAlign;
    }
    
    protected float m_startSpeed;
    protected float m_endSpeed;
    protected float m_duration;
    protected bool m_waitForAlign;

    public override void Update(float elapsedTime)
    {
 	    base.Update(elapsedTime);

      // calculate speed
      float speed = 0f;
      if (m_duration <= 0f)
      {
        speed = m_startSpeed;
      }
      else
      {
        speed = m_startSpeed + Math.Min(1.0f, m_time / m_duration) * (m_endSpeed - m_startSpeed);
      }

      // ensure YRotation is in -pi to pi
      float pi2 = (float)Math.PI * 2.0f;
      while (m_obj.CurrentTransform.YRotation < -(float)Math.PI)
      {
        m_obj.CurrentTransform.YRotation += pi2;
      }
      while (m_obj.CurrentTransform.YRotation > (float)Math.PI)
      {
        m_obj.CurrentTransform.YRotation -= pi2;
      }

      // save sign of rotation
      bool wasPositive = m_obj.CurrentTransform.YRotation > 0;

      // rotate
      m_obj.CurrentTransform.RotateRel(0.0f, 6.28318f * elapsedTime * speed, 0.0f);

      // check for finish
      if (m_waitForAlign)
      {
        if ((m_duration <= 0f) || (m_time >= m_duration))
        {
          if ( ((speed < 0f) && wasPositive && m_obj.CurrentTransform.YRotation < 0)
            || ((speed > 0f) && !wasPositive && m_obj.CurrentTransform.YRotation > 0) )
          {
            m_obj.CurrentTransform.YRotation = 0;
            m_finished = true;
          }
        }
      }
      else if ((m_duration > 0f) && (m_time >= m_duration))
      {
        m_finished = true;
      }
    }
  }


  public class ScaleAnimation : ObjAnimation
  {
    public ScaleAnimation(TextObj textObj, float endScale, float duration, float power)
      : base(textObj)
    {
      m_endScale = new Vector3(endScale, endScale, endScale);
      m_duration = duration;
      m_power = power;
    }

    protected Vector3 m_endScale;
    protected float m_duration;
    protected float m_power;
    protected Vector3 m_startScale;

    protected override void Init()
    {
      base.Init();

      m_startScale = new Vector3(m_obj.CurrentTransform.XScale, m_obj.CurrentTransform.YScale, m_obj.CurrentTransform.ZScale);
    }

    public override void Update(float elapsedTime)
    {
      base.Update(elapsedTime);

      // calculate speed
      float scaleFraction = 1f;
      if (m_duration > 0f)
      {
        scaleFraction = (float)Math.Pow(Math.Min(1f, m_time / m_duration), m_power);
      }

      Vector3 scale = m_startScale + scaleFraction * (m_endScale - m_startScale);

      // scale
      m_obj.CurrentTransform.ScaleAbs(scale.X, scale.Y, scale.Z);

      if (m_time >= m_duration)
      {
        m_finished = true;
      }
    }
  }


  public class CallAnimation : ObjAnimation
  {
    public CallAnimation(TextObj textObj)
      : base(textObj)
    {
      m_obj.CurrentTransform.YPosition = 1.5f;
      m_obj.CurrentTransform.ZPosition = 1000;
    }

    protected int m_part = 0;

    public override void Update(float elapsedTime)
    {
      base.Update(elapsedTime);

      // ensure YRotation is in -pi to pi
      float pi2 = (float)Math.PI * 2.0f;
      while (m_obj.CurrentTransform.YRotation < -(float)Math.PI)
      {
        m_obj.CurrentTransform.YRotation += pi2;
      }
      while (m_obj.CurrentTransform.YRotation > (float)Math.PI)
      {
        m_obj.CurrentTransform.YRotation -= pi2;
      }

      // segmented parts of animation
      if (m_part == 0)
      {
        if (m_time < 1)
        {
          // zoom in
          m_obj.CurrentTransform.ZPosition = 1000 * (float)Math.Pow(1 - m_time, 3);
          m_obj.CurrentTransform.RotateRel(0.0f, -3.14159f * elapsedTime * 15, 0.0f);
        }
        else
        {
          m_time = 0.0f;
          ++m_part;
        }
      }
      else if(m_part == 1)
      {
        if (m_time < 1)
        {
          // slow rotation
          m_obj.CurrentTransform.ZPosition = 0;
          m_obj.CurrentTransform.RotateRel(0.0f, -3.14159f * elapsedTime * (((1 - m_time) * 14) + 1), 0.0f);
        }
        else
        {
          m_time = 0.0f;
          ++m_part;
        }
      }
      else if(m_part == 2)
      {
        // stop rotation
        bool couldFinish = m_obj.CurrentTransform.YRotation > 0;
        m_obj.CurrentTransform.RotateRel(0.0f, -3.14159f * elapsedTime * 1.0f, 0.0f);
        if (couldFinish && m_obj.CurrentTransform.YRotation < 0)
        {
          m_obj.CurrentTransform.YRotation = 0;
          m_time = 0.0f;
          ++m_part;
        }
      }
      else if (m_part == 3)
      {
        if (m_time < 2)
        {
          float scale = -1 * (m_time - 1) * (m_time - 1) + 2;
          m_obj.CurrentTransform.ScaleAbs(scale, scale, scale);
        }
        else
        {
          m_time = 0.0f;
          ++m_part;
        }
      }
      else if (m_part == 4)
      {
        // move down to default position
        Vector3 dirToDest = m_obj.DefaultTransform.Position - m_obj.CurrentTransform.Position;
        float distToDest = dirToDest.Length();
        float moveDist = elapsedTime * 4.0f;
        if(moveDist > distToDest)
        {
          moveDist = distToDest;
          m_finished = true;
        }
        m_obj.CurrentTransform.Position += (dirToDest * moveDist);
      }
    }

  }
}
