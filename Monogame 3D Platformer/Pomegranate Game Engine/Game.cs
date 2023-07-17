using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public class GameManager
{
    public Camera MainCamera;
    public World world;
    public List<PomModel> CurrentLevel = new List<PomModel>();
    public List<PomModel> Tree = new List<PomModel>();
    public List<PomModel> Bog = new List<PomModel>();
    public PomModel Shadow;
    public PomModel BogLeftFoot;
    public PomModel BogRightFoot;
    public Vector3 BogPosition;
    public Vector3 BogForward;
    public float WalkDistance;
    public float BogRotation;
    public float BogRotationVelocity;
    public Vector3 BogVelocity;
    public void Init(ContentManager Content)
    {
        MainCamera = new Camera(Content, Engine.device, 1920/4, 1080/4);

        MainCamera.RenderDistance = 1000f;
        MainCamera.Position = new Vector3(0, 0, -3);
        MainCamera.Rotation = new Vector3(0, 180, 0);
        MainCamera.FOV = 70;
        world = new World(Vector3.Down,1,Color.DeepSkyBlue);
        CurrentLevel.Add(PomModel.FromFile(File.ReadAllText("Resources/Level1.Grass.obj"),Texture2D.FromFile(Engine.device,"Textures/grass_1.png")));
        CurrentLevel.Add(PomModel.FromFile(File.ReadAllText("Resources/Level1.Cobble_Stone.obj"), Texture2D.FromFile(Engine.device, "Textures/Cobble.png")));
        CurrentLevel.Add(PomModel.FromFile(File.ReadAllText("Resources/Level1.Cobble_Wall.obj"), Texture2D.FromFile(Engine.device, "Textures/Cobble_Wall.png")));
        //CurrentLevel.Add(PomModel.FromFile(File.ReadAllText("Resources/Level1.Grass_0.obj"), Texture2D.FromFile(Engine.device, "Textures/grass_0.png")));
        Tree.Add(PomModel.FromFile(File.ReadAllText("Resources/Tree.Wood.obj"), Texture2D.FromFile(Engine.device, "Textures/Wood_0.png")));
        Tree.Add(PomModel.FromFile(File.ReadAllText("Resources/Tree.Leaves.obj"), Texture2D.FromFile(Engine.device, "Textures/grass_1.png")));

        Bog.Add(PomModel.FromFile(File.ReadAllText("Resources/Bog.Body.obj"), Engine.NullTexture));
        BogLeftFoot = (PomModel.FromFile(File.ReadAllText("Resources/Bog.Foot.Left.obj"), Engine.NullTexture));
        BogRightFoot = (PomModel.FromFile(File.ReadAllText("Resources/Bog.Foot.Right.obj"), Engine.NullTexture));
        Shadow = (PomModel.FromFile(File.ReadAllText("Resources/ShadowCircle.obj"), Engine.NullTexture));
        Bog.Add(PomModel.FromFile(File.ReadAllText("Resources/Bog.Eyes.obj"), Engine.NullTexture));
        BogPosition = Vector3.One;
        BogForward = Vector3.Forward;
    }
    public void Update(GameTime gameTime)
    {
        KeyboardState kbs = Keyboard.GetState();
        float sensitivity = 2;
        MainCamera.Position = Vector3.Forward*-5f;
        if (kbs.IsKeyDown(Keys.Left))
        {
            MainCamera.Rotation += new Vector3(0, -1, 0) * sensitivity;
        }
        if (kbs.IsKeyDown(Keys.Right))
        {
            MainCamera.Rotation += new Vector3(0, 1, 0) * sensitivity;
        }
        if (kbs.IsKeyDown(Keys.Up))
        {
            MainCamera.Rotation += new Vector3(-1, 0, 0) * sensitivity;
        }
        if (kbs.IsKeyDown(Keys.Down))
        {
            MainCamera.Rotation += new Vector3(1, 0, 0) * sensitivity;
        }
        if(kbs.IsKeyDown(Keys.A))
        {
            BogRotationVelocity -= 0.02f;
        }
        if (kbs.IsKeyDown(Keys.D))
        {
            BogRotationVelocity += 0.02f;
        }
        if(kbs.IsKeyDown(Keys.W))
        {
            BogVelocity += BogForward*0.05f;
        }
        if (kbs.IsKeyDown(Keys.S))
        {
            BogVelocity += BogForward * -0.05f;
        }
        if(kbs.IsKeyDown(Keys.Space) && BogPosition.Y <= 0.5f)
        {
            BogVelocity.Y = 0.2f;
        }
        BogRotationVelocity *= 0.8f;
        BogRotation -= BogRotationVelocity;
        MainCamera.Rotation.Y -= MathHelper.ToDegrees(BogRotationVelocity);
        BogForward = new Vector3(MathF.Cos(-BogRotation), 0, MathF.Sin(-BogRotation));
        Console.WriteLine(BogForward);

        BogVelocity.X *= 0.8f;
        BogVelocity.Z *= 0.8f;
        BogVelocity.Y -= 0.01f;
        BogPosition += BogVelocity;
        if(BogPosition.Y <= 0.5f)
        WalkDistance += new Vector2(BogVelocity.X,BogVelocity.Z).Length();
        else
        {
            WalkDistance = 0;
        }
        if(BogPosition.Y < 0.5f)
        {
            BogPosition.Y = 0.5f;
            BogVelocity.Y = 0;
        }    
        MainCamera.Position = Vector3.Transform(MainCamera.Position, Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(MainCamera.Rotation.Y), MathHelper.ToRadians(MainCamera.Rotation.X), MathHelper.ToRadians(MainCamera.Rotation.Z)));
        MainCamera.Position += BogPosition + new Vector3(0, MathF.Sin(WalkDistance * 2), 0) * 0.05f;
    }
    public void Draw()
    {
        Engine.device.Clear(Color.SkyBlue);
        world.ClearVertexData();
        world.SetVertexData(Engine.device,CurrentLevel.ToArray(),Vector3.Zero, Matrix.Identity);
        world.SetVertexData(Engine.device, Bog.ToArray(), BogPosition + new Vector3(0, MathF.Sin(WalkDistance * 2), 0) * 0.15f, Matrix.CreateLookAt(Vector3.Zero, new Vector3(MathF.Cos(BogRotation), 0, MathF.Sin(BogRotation)), Vector3.Up));
        world.SetVertexData(Engine.device, BogLeftFoot, BogPosition + new Vector3(0, MathF.Max(MathF.Min(MathF.Sin(WalkDistance * 1) * 1, 2), 0) + BogVelocity.Y*5, 0) * 0.3f, Matrix.CreateLookAt(Vector3.Zero, new Vector3(MathF.Cos(BogRotation), 0, MathF.Sin(BogRotation)), Vector3.Up));
        world.SetVertexData(Engine.device, BogRightFoot, BogPosition + new Vector3(0, MathF.Max(MathF.Min(MathF.Sin(WalkDistance*1+ 3.14f) * 1, 2), 0) + BogVelocity.Y*5, 0) * 0.3f, Matrix.CreateLookAt(Vector3.Zero, new Vector3(MathF.Cos(BogRotation), 0, MathF.Sin(BogRotation)), Vector3.Up));
        world.SetVertexData(Engine.device, Shadow, new Vector3(BogPosition.X, 0.01f, BogPosition.Z), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(-10, 0, -7), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(-10, 0, 7), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(7, 0, -4), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(4, 0, 6), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(7, 0, 9), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(8, 0, -3), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(-6, 0, -2), Matrix.Identity);
        world.SetVertexData(Engine.device, Tree.ToArray(), new Vector3(7, 0, 4), Matrix.Identity);
        Engine.RenderToScreen(Engine.device,world,MainCamera);
    }
}
