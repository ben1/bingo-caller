using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Generic;
using Microsoft.DirectX.Direct3D.CustomVertex;
using System.Drawing;

namespace CUnit
{
  public class TextObj
  {
    public TextObj(string text)
    {
      m_text = text;
      m_animSet = new AnimationSet(null);
      m_currentTransform.Position = new Vector3(0f, 1.5f, 1000f);
    }

    public string Text
    {
      get { return m_text; }
      set
      {
        m_text = value;
        if (m_text == null)
        {
          m_mesh = null;
        }
        else if ((m_mesh != null) && (m_device != null))
        {
          m_mesh = Microsoft.DirectX.Direct3D.Mesh.TextFromFont(m_device, new System.Drawing.Font("Arial", 16), m_text, 0.002f, 0.15f);
        }
      }
    }
    protected string m_text;

    public WorldTransform DefaultTransform
    {
      get { return m_defaultTransform; }
    }
    protected WorldTransform m_defaultTransform = new WorldTransform();

    public WorldTransform CurrentTransform
    {
      get { return m_currentTransform; }
      set { m_currentTransform = value; }
    }
    protected WorldTransform m_currentTransform = new WorldTransform();

    public bool Visible = false;

    protected Device m_device;
    protected Effect m_effect;
    protected Microsoft.DirectX.Direct3D.Mesh m_mesh;
    protected ColorValue m_color = ColorValue.FromColor(Color.Red);
    protected Vector3 m_centreOffset = new Vector3();

    public bool IsAnimating { get { return !m_animSet.IsFinished; } }
    protected AnimationSet m_animSet;

    public void OnResetDevice(Device device, Effect effect)
    {
      m_device = device;
      m_effect = effect;

      m_mesh = Microsoft.DirectX.Direct3D.Mesh.TextFromFont(device, new System.Drawing.Font("Arial", 16), m_text, 0.005f, 0.15f);
      // find min and max xyz
      Vector3 min = new Vector3(1000.0f, 1000.0f, 1000.0f);
      Vector3 max = new Vector3(-1000.0f, -1000.0f, -1000.0f);
      GraphicsBuffer<PositionNormal> buffer = m_mesh.VertexBuffer.Lock<PositionNormal>(0, m_mesh.VertexCount, LockFlags.ReadOnly);
      IEnumerator<PositionNormal> i = buffer.GetEnumerator();
      while (i.MoveNext())
      {
        min.X = Math.Min(min.X, i.Current.Position.X);
        min.Y = Math.Min(min.Y, i.Current.Position.Y);
        min.Z = Math.Min(min.Z, i.Current.Position.Z);
        max.X = Math.Max(max.X, i.Current.Position.X);
        max.Y = Math.Max(max.Y, i.Current.Position.Y);
        max.Z = Math.Max(max.Z, i.Current.Position.Z);
      }
      m_mesh.VertexBuffer.Unlock();
      buffer.Dispose();

      // calculate centre offset
      m_centreOffset.X = (max.X + min.X) / -2.0f;
      m_centreOffset.Y = (max.Y + min.Y) / -2.0f;
      m_centreOffset.Z = (max.Z + min.Z) / -2.0f;

    }

    public void OnlostDevice()
    {
      m_device = null;

      if (m_mesh != null)
      {
        m_mesh.Dispose();
        m_mesh = null;
      }

      m_effect = null;
    }

    public virtual void Update(float elapsedTime)
    {
      m_animSet.Update(elapsedTime);
    }

    public virtual void Render(Device device)
    {
      if (Visible && (m_mesh != null))
      {
        Matrix offset = Matrix.Identity;
        offset.M41 = m_centreOffset.X;
        offset.M42 = m_centreOffset.Y;
        offset.M43 = m_centreOffset.Z;

        Matrix world = offset * m_currentTransform.Transform;
        device.Transform.World = world;

        m_effect.SetValue("g_mWorld", world);
        m_effect.SetValue("g_matDiffuse", m_color);


        int passes = m_effect.Begin(0);
        for (int pass = 0; pass < passes; pass++)
        {
          m_effect.BeginPass(pass);

          // Since we're setting values between the BeginPass/EndPass calls, we 
          // must call CommitChanges before the call to draw
          // m_effect.CommitChanges();
          m_mesh.DrawSubset(0);

          m_effect.EndPass();
        }
        m_effect.End();
      }
    }

    public void AddAnimation(Animation anim)
    {
      m_animSet.AddAnimation(anim);
    }

    public void RemoveAnimation(Animation anim)
    {
      m_animSet.RemoveAnimation(anim);
    }
  }
}
