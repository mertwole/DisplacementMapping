using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;

namespace Tesselation
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main()
        {
            Game game = new Game();
            game.Run();
        }

        static int window_width = 600;
        static int window_height = 600;

        public Game() : base(window_width, window_height, GraphicsMode.Default, "Tesselation")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnResize(EventArgs E)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
        }

        int render_shader;

        protected override void OnLoad(EventArgs E)
        {
            GL.ClearColor(Color.Black);

            render_shader = new ShaderProgram()
            .addVertexShader(new System.IO.StreamReader("vert.glsl"))
            .addFragmentShader(new System.IO.StreamReader("frag.glsl"))
            .addTessEvaluationShader(new System.IO.StreamReader("tessev.glsl"))
            .Compile();

            GL.UseProgram(render_shader);

            int VAO = GL.GenVertexArray();
            int VBO = GL.GenBuffer();

            float[] verts = new float[]// pos tex norm tangent
            {
                -1, -1, 0,   0, 0,   0, 0, 1,   0, 1, 0,
                -1,  1, 0,   0, 1,   0, 0, 1,   0, 1, 0,
                 1,  1, 0,   1, 1,   0, 0, 1,   0, 1, 0,
                 1, -1, 0,   1, 0,   0, 0, 1,   0, 1, 0,
            };

            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * verts.Length, verts, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 11, sizeof(float) * 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 11, sizeof(float) * 3);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, sizeof(float) * 11, sizeof(float) * 5);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(float) * 11, sizeof(float) * 8);
            GL.EnableVertexAttribArray(3);

            #region textures
            var ReadOnly = System.Drawing.Imaging.ImageLockMode.ReadOnly;

            Bitmap albedo = (Bitmap)Image.FromFile("resources/albedo.bmp");
            Rectangle albedo_rect = new Rectangle(0, 0, albedo.Width, albedo.Height);
            var albedo_data = albedo.LockBits(albedo_rect, ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int albedo_tex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, albedo_tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, albedo.Width, albedo.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, albedo_data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            albedo.UnlockBits(albedo_data);
            albedo.Dispose();

            Bitmap depth = (Bitmap)Image.FromFile("resources/depth.bmp");
            Rectangle depth_rect = new Rectangle(0, 0, depth.Width, depth.Height);
            var depth_data = depth.LockBits(depth_rect, ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int depth_tex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depth_tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, depth.Width, depth.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, depth_data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            depth.UnlockBits(depth_data);
            depth.Dispose();

            Bitmap normal = (Bitmap)Image.FromFile("resources/normal.bmp");
            Rectangle normal_rect = new Rectangle(0, 0, normal.Width, normal.Height);
            var normal_data = normal.LockBits(normal_rect, ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int normal_tex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, normal_tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, normal.Width, normal.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, normal_data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            normal.UnlockBits(normal_data);
            normal.Dispose();
            #endregion

            GL.PatchParameter(PatchParameterInt.PatchVertices, 4);
            GL.PatchParameter(PatchParameterFloat.PatchDefaultInnerLevel, new float[] { 64, 64 });
            GL.PatchParameter(PatchParameterFloat.PatchDefaultOuterLevel, new float[] { 64, 64, 64, 64 });
        }

        Camera camera = new Camera(new Vector3(0, 0, 10), 0, -(float)Math.PI / 2);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)window_width / (float)window_height, 0.1f, 1000);

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Matrix4 transform = camera.Matrix * projection;
            GL.ProgramUniformMatrix4(render_shader, GL.GetUniformLocation(render_shader, "transform_mat"), false, ref transform);

            Vector3 viewer_pos = camera.Pos;
            GL.Uniform3(GL.GetUniformLocation(render_shader, "viewer_pos"), ref viewer_pos);

            GL.DrawArrays(PrimitiveType.Patches, 0, 4);

            SwapBuffers();
            camera.Update(0.05f);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            camera.KeyboardEvents(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            camera.KeyboardEvents(e);
        }
    }
}