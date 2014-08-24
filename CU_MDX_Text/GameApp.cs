using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.Generic;
using Microsoft.DirectX.Direct3D.CustomVertex;

namespace CUnit
{
    /// <summary>Main application</summary>
    public class GameApp : GameState
    {
        private Framework m_framework = null;
        private const string Instructions = "Esc: Quit";
        private string m_instructionDisplay = "F1: View instructions";
        public enum ControlID { Fullscreen, Options };
        private float m_fps = 0f;
        private Gui.GuiManager m_gui = null;
        private BitmapFont m_bFont = null;

        // Application-specific variables
        private Sprite m_textSprite = null;
        private Font m_font = null;
        private NumObj[] m_numObjs = new NumObj[75];
        private Caller m_state = new Caller();
        private TextObj m_callText;
        private Effect m_numEffect;
      private Matrix m_camView;
      private Matrix m_camProj;
      private bool m_firstCallMade = false;

        /// <summary>Default constructor.</summary>
        public GameApp( Framework framework )
        {
            m_framework = framework;
            m_framework.PushState( this );
        }

        /// <summary>Initialize application-specific resources, states here.</summary>
        /// <returns>True on success, false on failure</returns>
        private bool Initialize()
        {
            m_bFont = new BitmapFont( "Arial38.fnt", "Arial38_00.tga" );

            for(int i = 0; i < m_numObjs.Length; ++i)
            {
              m_numObjs[i] = new NumObj(i + 1);
            }
            m_callText = new TextObj("BINGO!");
            m_callText.Visible = true;
            m_callText.DefaultTransform.Position = new Vector3(0.0f, 1.7f, 0.0f);
            float scale = 1.6f;
            m_callText.DefaultTransform.ScaleAbs(scale, scale, scale);
            m_callText.CurrentTransform.Set(m_callText.DefaultTransform);

            return true;
        }

        /// <summary>
        /// This event will be fired immediately after the Direct3D device has been 
        /// created, which will happen during application initialization. This is the best 
        /// location to create Pool.Managed resources since these resources need to be 
        /// reloaded whenever the device is destroyed. Resources created  
        /// here should be released in the OnDetroyDevice callback. 
        /// </summary>
        /// <param name="device">The Direct3D device</param>
        public override void OnCreateDevice( Device device )
        {
            if ( m_gui != null )
            {
                m_gui.Clear();
            }

            m_gui = new Gui.GuiManager( "CUnit.xml", new Gui.GuiManager.ControlDelegate( OnControl ) );
            m_gui.CreateButton( (int)ControlID.Fullscreen, 0, new PointF( (float)BackBufferWidth - 120f, 10f ), new SizeF( 110f, 30f ), "Toggle Fullscreen", 15f, new ColorValue( 0, 0, 0 ) );
            m_gui.CreateButton( (int)ControlID.Options, 0, new PointF( (float)BackBufferWidth - 120f, 50f ), new SizeF( 110f, 30f ), "Options", 15f, new ColorValue( 0, 0, 0 ) );
            m_gui.OnCreateDevice( device );

            if ( m_bFont != null )
            {
                m_bFont.OnCreateDevice( device );
            }

            m_font = new Font( device, "Arial", 12, false, false );
            m_textSprite = new Sprite( device );
            
        }

        /// <summary>
        /// This event will be fired immediately after the Direct3D device has been 
        /// reset, which will happen after a lost device scenario, a window resize, and a 
        /// fullscreen toggle. This is the best location to create Pool.Default resources 
        /// since these resources need to be reloaded whenever the device is reset. Resources 
        /// created here should be released in the OnLostDevice callback. 
        /// </summary>
        /// <param name="device">The Direct3D device</param>
        public override void OnResetDevice( Device device )
        {
            if ( m_gui != null )
            {
                m_gui.OnResetDevice( device );
            }
            if ( m_bFont != null )
            {
                m_bFont.OnResetDevice( device );
            }
            if ( m_font != null )
            {
                m_font.OnResetDevice();
            }
            if ( m_textSprite != null )
            {
                m_textSprite.OnResetDevice();
            }

            // Set transforms
            Vector3 cameraPosition = new Vector3( 0f, 0f, -5f );
            Vector3 cameraTarget = new Vector3( 0f, 0f, 0f );
            Vector3 cameraUp = new Vector3( 0f, 1f, 0f );
            m_camView = Matrix.LookAtLeftHanded( cameraPosition, cameraTarget, cameraUp );
            device.Transform.View = m_camView;
            Size displaySize = m_framework.DisplaySize;
            float aspect = (float)displaySize.Width / (float)displaySize.Height;
            m_camProj = Matrix.PerspectiveFieldOfViewLeftHanded( (float)Math.PI / 3.0f, aspect, 0.1f, 1000.0f );
            device.Transform.Projection = m_camProj;

            m_numEffect = Effect.FromFile(device, "../../media/numeffect.fx", null, null, null, ShaderFlags.None, null);


            // Set render states
            device.RenderState.Lighting = false;

            // Create our 3d text mesh
            for (int i = 0; i < 75; ++i)
            {
              m_numObjs[i].OnResetDevice(device, m_numEffect);
            }

            m_callText.OnResetDevice(device, m_numEffect);

            m_gui.SetPosition( (int)ControlID.Fullscreen, new PointF( (float)BackBufferWidth - 120f, 10f ) );
            m_gui.SetPosition( (int)ControlID.Options, new PointF( (float)BackBufferWidth - 120f, 50f ) );
        }

        /// <summary>
        /// This function will be called after the Direct3D device has 
        /// entered a lost state and before Device.Reset() is called. Resources created
        /// in the OnResetDevice callback should be released here, which generally includes all 
        /// Pool.Default resources.
        /// </summary>
        public override void OnLostDevice()
        {
            if ( m_gui != null )
            {
                m_gui.OnLostDevice();
            }
            if ( m_bFont != null )
            {
                m_bFont.OnLostDevice();
            }
            if ( m_font != null )
            {
                m_font.OnLostDevice();
            }
            if ( m_textSprite != null && !m_textSprite.IsDisposed )
            {
                m_textSprite.OnLostDevice();
            }
            for (int i = 0; i < 75; ++i)
            {
              m_numObjs[i].OnlostDevice();
            }
            m_callText.OnlostDevice();
            if (m_numEffect != null)
            {
              m_numEffect.Dispose();
              m_numEffect = null;
            }
        }

        /// <summary>
        /// This callback function will be called immediately after the Direct3D device has 
        /// been destroyed, which generally happens as a result of application termination. 
        /// Resources created in the OnCreateDevice callback should be released here, which 
        /// generally includes all Pool.Managed resources. 
        /// </summary>
        public override void OnDestroyDevice()
        {
            if ( m_gui != null )
            {
                m_gui.OnDestroyDevice();
            }
            if ( m_bFont != null )
            {
                m_bFont.OnDestroyDevice();
            }
            if ( m_font != null )
            {
                m_font.OnDestroyDevice();
                m_font = null;
            }
            if ( m_textSprite != null )
            {
                m_textSprite.Dispose();
                m_textSprite = null;
            }
        }

        /// <summary>Updates a frame prior to rendering.</summary>
        /// <param name="device">The Direct3D device</param>
        /// <param name="elapsedTime">Time elapsed since last frame</param>
        public override void OnUpdateFrame( Device device, float elapsedTime )
        {
          foreach (NumObj n in m_numObjs)
          {
            n.Update(elapsedTime);
          }
          m_callText.Update(elapsedTime);

          if (m_firstCallMade && !m_callText.IsAnimating)
          {
            m_firstCallMade = false;
            Call();
          }
        }

        /// <summary>Renders the current frame.</summary>
        /// <param name="device">The Direct3D device</param>
        /// <param name="elapsedTime">Time elapsed since last frame</param>
        public override void OnRenderFrame( Device device, float elapsedTime )
        {
          device.Clear( ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0 );
          device.BeginScene();

          // Set render states since GUI and font changes them when they render
          device.SetRenderState( RenderStates.ZEnable, true );
          device.SetRenderState( RenderStates.ZBufferWriteEnable, true );
          device.SetRenderState( RenderStates.AlphaBlendEnable, false );
          
          device.RenderState.FillMode = m_framework.FillMode;

          m_numEffect.SetValue("g_fTime", m_framework.Timer.RunningTime);
          m_numEffect.SetValue("g_mViewProjection", m_camView * m_camProj);
          m_numEffect.SetValue("g_lightPos", new Vector4(-2f, 2f, -2f, 1f));
          m_numEffect.SetValue("g_light2Pos", new Vector4(2f, 2f, -2f, 1f));
          m_numEffect.SetValue("g_lightAmbient", new Vector4(0.2f, 0f, 0f, 0f));

          // Render 3D text
          foreach (NumObj n in m_numObjs)
          {
            n.Render(device);
          }

          m_callText.Render(device);

          // Render GUI
#if DEBUG
          m_gui.Render( device );
#endif
          // Only need to rebuild the text when the FPS updates
          if ( m_fps != m_framework.FPS )
          {
              m_fps = m_framework.FPS;
              BuildText();
          }
//          m_bFont.Render( device );

          device.EndScene();
          device.Present();
        }

        /// <summary>Builds all the BitmapFont strings.</summary>
        private void BuildText()
        {
            m_bFont.ClearStrings();
            m_bFont.AddString( "FPS: " + m_fps.ToString( "f2" ), new RectangleF( 5f, 5f, (float)BackBufferWidth, 100f ), BitmapFont.Align.Left, 16f, ColorValue.FromColor( Color.Red ), true );
            m_bFont.AddString( m_instructionDisplay, new RectangleF( 5f, 20f, 500f, 500f ), BitmapFont.Align.Left, 16f, ColorValue.FromColor( Color.White ), true );
        }

        /// <summary>Keyboard event handler</summary>
        /// <param name="pressedKeys">List of pressed keys</param>
        /// <param name="pressedChar">Unicode character read from Windows Form</param>
        /// <param name="pressedKey">Pressed key from Form used for repeatable keys</param>
        /// <param name="elapsedTime">Time since last frame</param>
        public override void OnKeyboard( List<Keys> pressedKeys, char pressedChar, int pressedKey, float elapsedTime )
        {
            if ( m_gui.KeyBoardHandler( pressedKeys, pressedChar, pressedKey ) )
            {
               return;
            }

            foreach ( Keys k in pressedKeys )
            {
                switch ( k )
                {
                    case Keys.Escape:
                        m_framework.Close();
                        break;
                    case Keys.F1:
                        m_framework.LockKey( Keys.F1 );
                        m_instructionDisplay = ( m_instructionDisplay == Instructions ) ? "F1: View instructions" : Instructions;
                        BuildText();
                        break;
                  case Keys.Space:
                    m_framework.LockKey(Keys.Space);
                    if (!m_state.Finished)
                    {
                      if (m_state.LastNumber == 0)
                      {
                        if (!m_callText.IsAnimating)
                        {
                          Animation[] anims2 =
                          {
                            new ZoomAnim(m_callText, new Vector3(0f, 0f, 1000f), 1.5f, 5f),
                            new SpinAnimation(m_callText, 2f, 7.5f, 1.5f, false)
                          };
                          Animation[] anims3 = 
                          {
                            new SpinAnimation(m_callText, 0f, 2f, 1f, false),
                            new AnimationSetQuickFinish(anims2)
                          };

                          m_callText.AddAnimation(new AnimationSetOrdered(anims3));
                          m_firstCallMade = true;
                        }
                      }
                      else
                      {
                        Call();
                      }
                    }
                    break;
                }
            }
        }

        /// <summary>Mouse event handler</summary>
        /// <param name="position">Mouse position in client coordinates</param>
        /// <param name="xDelta">X-axis delta</param>
        /// <param name="yDelta">Y-axis delta</param>
        /// <param name="zDelta">Mouse wheel delta</param>
        /// <param name="buttons">Mouse buttons</param>
        public override void OnMouse( Point position, int xDelta, int yDelta, int zDelta, bool[] buttons, float elapsedTime )
        {
            if ( m_gui.MouseHandler( position, buttons, zDelta ) )
            {
                return;
            }
        }

        /// <summary>Gui Control handler</summary>
        /// <param name="controlID">Control ID</param>
        /// <param name="data">Control data</param>
        public override void OnControl( int controlID, object data )
        {
            switch ( controlID )
            {
                case (int)ControlID.Fullscreen:
                    m_framework.ToggleFullscreen();
                    break;
                case (int)ControlID.Options:
                    m_framework.PushState( new DeviceOptionsDialog( m_framework ) );
                    break;
            }
        }

        /// <summary>
        /// Inherited method from GameState. Determines whether 
        /// the StateManager should pop this state.
        /// </summary>
        public override bool DoneWithState
        {
            get { return false; }
        }

        /// <summary>The main entry point for the application.</summary>
        [STAThread]
        static void Main() 
        {
            using ( Framework framework = new Framework() )
            {
                GameApp app = new GameApp( framework );

                try
                {
                    app.Initialize();
#if DEBUG
                    framework.Initialize( true, 640, 480, "Bingo" );
#else
                    framework.Initialize( false, 1400, 1050, "Bingo" );
#endif
                    framework.Run();
                }
                catch ( Exception e )
                {
                    framework.Close();
                    System.Windows.Forms.MessageBox.Show( e.ToString(), "Error" );
                    return;
                }
            }
        }

        /// <summary>Gets the Device's current back buffer height</summary>
        public int BackBufferHeight
        {
            get { return m_framework.CurrentSettings.PresentParameters.BackBufferHeight; }
        }

        /// <summary>Gets the Device's current back buffer width</summary>
        public int BackBufferWidth
        {
            get { return m_framework.CurrentSettings.PresentParameters.BackBufferWidth; }
        }

        private void Call()
        {
          m_state.Call();
          if (m_state.Finished)
          {
            if (!m_callText.IsAnimating)
            {
              m_callText.Text = "Finished!";
              float zoomTime = 1.5f;
              Animation[] anims = 
              {
                new ZoomAnim(m_callText, m_callText.DefaultTransform.Position, zoomTime, 0.2f), 
                new SpinAnimation(m_callText, 7.5f, 7.5f, zoomTime, true) 
              };

              AnimationSet animSet = new AnimationSet(anims);
              m_callText.AddAnimation(animSet);
            }
          }
          else if (m_state.LastNumber == 0)
          {
            m_callText.Text = "BINGO!";
            m_callText.Visible = true;
          }
          else
          {
            NumObj obj = m_numObjs[m_state.LastNumber - 1];
            obj.Visible = true;

            Animation[] zoomInWhileRotate =
            {
              new ZoomAnim(obj, new Vector3(0f, 1.5f, 0f), 1f, 0.3f),
              new SpinAnimation(obj, 7.5f, 0f, 0f, false)
            };

            float scaleDownWhileMoveDuration = 1f;
            Animation[] scaleDownWhileMove =
            {
              new ScaleAnimation(obj, 1f, scaleDownWhileMoveDuration, 2f),
              new ZoomAnim(obj, obj.DefaultTransform.Position, scaleDownWhileMoveDuration, 1f)
            };

            Animation[] anims = 
            {
              new ZoomAnim(obj, new Vector3(0f, 1.5f, 1000f), 0f, 1f),
              new AnimationSetQuickFinish(zoomInWhileRotate),
              new SpinAnimation(obj, 7.5f, 1f, 1f, true),
              new ScaleAnimation(obj, 2f, 0.5f, 0.5f),
              new AnimationSet(scaleDownWhileMove)
            };

            AnimationSetOrdered animSet = new AnimationSetOrdered(anims);
            obj.AddAnimation(animSet);
          }
        }
    }
}