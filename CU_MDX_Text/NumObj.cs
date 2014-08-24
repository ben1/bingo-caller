using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Generic;
using Microsoft.DirectX.Direct3D.CustomVertex;


namespace CUnit
{
  public class NumObj : TextObj
  {
    public NumObj(int a_num) : base((a_num).ToString())
    {
      m_num = a_num;
      int column = (a_num - 1) % 10;
      int row = (a_num - 1) / 10;
      
      m_defaultTransform.XPosition = (column - 4.5f) * 1.5f;
      m_defaultTransform.YPosition = row * -0.8f + 0.5f;
      m_defaultTransform.ZPosition = 6.0f;

      m_color = s_colors[s_rand.Next(s_colors.Length)];
    }

    int RandColor()
    {
      int col = 0xFF << 24;
      col += (32 + s_rand.Next(224)) << 16;
      col += (32 + s_rand.Next(224)) << 8;
      col += (32 + s_rand.Next(224)) ;
      return col;
    }

    int m_num;

    static Random s_rand = new Random();

    static ColorValue[] s_colors = 
    { 
      ColorValue.FromColor(Color.AliceBlue),
      ColorValue.FromColor(Color.BlueViolet),
      ColorValue.FromColor(Color.CornflowerBlue),
      ColorValue.FromColor(Color.Crimson),
      ColorValue.FromColor(Color.Cornsilk),
      ColorValue.FromColor(Color.DeepSkyBlue),
      ColorValue.FromColor(Color.Firebrick),
      ColorValue.FromColor(Color.ForestGreen),
      ColorValue.FromColor(Color.Honeydew),
      ColorValue.FromColor(Color.IndianRed),
      ColorValue.FromColor(Color.Indigo),
      ColorValue.FromColor(Color.MediumSpringGreen),
      ColorValue.FromColor(Color.Orange),
      ColorValue.FromColor(Color.Purple),
      ColorValue.FromColor(Color.SeaGreen),
      ColorValue.FromColor(Color.Yellow)
    };
  }
}
