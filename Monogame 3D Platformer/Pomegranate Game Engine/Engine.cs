using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
﻿using System.Runtime.InteropServices;
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionColorNormalTexture : IVertexType
{
    public Vector3 Position;
    public Color Color;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;
    public static readonly VertexDeclaration VertexDeclaration;

    public VertexPositionColorNormalTexture(Vector3 position, Color color, Vector3 normal, Vector2 textureCoordinate)
    {
        Position = position;
        Color = color;
        Normal = normal;
        TextureCoordinate = textureCoordinate;
    }

    VertexDeclaration IVertexType.VertexDeclaration
    {
        get
        {
            return VertexDeclaration;
        }
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Position.GetHashCode();
            hashCode = (hashCode * 397) ^ Color.GetHashCode();
            hashCode = (hashCode * 397) ^ Normal.GetHashCode();
            hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
            return hashCode;
        }
    }

    public override string ToString()
    {
        return "{{Position:" + this.Position + " Color:" + this.Color + " Normal:" + this.Normal + " TextureCoordinate:" + this.TextureCoordinate + "}}";
    }

    public static bool operator ==(VertexPositionColorNormalTexture left, VertexPositionColorNormalTexture right)
    {
        return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.Normal == right.Normal) && (left.TextureCoordinate == right.TextureCoordinate));
    }

    public static bool operator !=(VertexPositionColorNormalTexture left, VertexPositionColorNormalTexture right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() != base.GetType())
            return false;

        return (this == ((VertexPositionColorNormalTexture)obj));
    }

    static VertexPositionColorNormalTexture()
    {
        var elements = new VertexElement[]
        {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        };
        VertexDeclaration = new VertexDeclaration(elements);
    }
}
public struct PomModel
{
    public Texture2D texture;
    public VertexPositionColorNormalTexture[] Vertices;
    public short[] Indices;
    public static PomModel FromFile(string contents, Texture2D material_texture)
    {
        List<VertexPositionColorNormalTexture> vertices = new List<VertexPositionColorNormalTexture>();
        List<short> indices = new List<short>();
        List<Vector2> textureCoords = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        string[] lines = contents.Split('\n');

        foreach (string line in lines)
        {
            string[] tokens = line.Trim().Split(' ');

            if (tokens.Length == 0 || tokens[0] == "#" || tokens[0] == "")
            {
                continue;
            }
            else if (tokens[0] == "v")
            {
                float x = float.Parse(tokens[1]);
                float y = float.Parse(tokens[2]);
                float z = float.Parse(tokens[3]);
                float r = 1f;
                float g = 1f;
                float b = 1f;
                if (tokens.Length > 4)
                {
                    r = float.Parse(tokens[4]);
                    g = float.Parse(tokens[5]);
                    b = float.Parse(tokens[6]);
                }

                Vector3 position = new Vector3(x, y, z);

                VertexPositionColorNormalTexture vertex = new VertexPositionColorNormalTexture(position, new Color(r,g,b),Vector3.Zero, Vector2.Zero);
                vertices.Add(vertex);
            }
            else if (tokens[0] == "vt")
            {
                float u = float.Parse(tokens[1]);
                float v = float.Parse(tokens[2]);
                Vector2 textureCoord = new Vector2(u, v);
                textureCoords.Add(textureCoord);
            }
            else if (tokens[0] == "vn")
            {
                float nx = float.Parse(tokens[1]);
                float ny = float.Parse(tokens[2]);
                float nz = float.Parse(tokens[3]);
                Vector3 normal = new Vector3(nx, ny, nz);
                normals.Add(normal);
            }
            else if (tokens[0] == "f")
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    string[] indexTokens = tokens[i].Split('/');
                    int positionIndex = int.Parse(indexTokens[0]) - 1;
                    int textureIndex = int.Parse(indexTokens[1]) - 1;
                    int normalIndex = int.Parse(indexTokens[2]) - 1;

                    indices.Add((short)positionIndex);

                    VertexPositionColorNormalTexture vertex = vertices[positionIndex];
                    vertex.TextureCoordinate = textureCoords[textureIndex];
                    vertex.Normal = normals[normalIndex];
                    vertices[positionIndex] = vertex;
                }
            }
        }
        return new PomModel
        {
            texture = material_texture,
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray()
        };
    }
}
public class World
{
    public Color SkyColor;
    public List<Texture2D> Textures = new List<Texture2D>();
    public List<VertexBuffer> vertexBuffer = new List<VertexBuffer>();
    public List<IndexBuffer> indexBuffer = new List<IndexBuffer>();
    public Vector3 SunDirection;
    public float Intensity;
    public World(Vector3 SunDirection, float Intensity, Color SkyColor)
    {
        this.SunDirection = SunDirection;
        this.Intensity = Intensity;
        this.SkyColor = SkyColor;
    }
    public void ClearVertexData()
    {
        for (int i = 0; i < vertexBuffer.Count; i++)
        {
            vertexBuffer[i].Dispose();
            indexBuffer[i].Dispose();
        }
        vertexBuffer.Clear();
        indexBuffer.Clear();
    }
    public void SetVertexData(GraphicsDevice graphics, PomModel model, Vector3 Offset, Matrix Rotation)
    {
        Textures.Add(model.texture);
        model.Vertices = (VertexPositionColorNormalTexture[])model.Vertices.Clone();
        if (Offset != Vector3.Zero)
        {
            for (int i = 0; i < model.Vertices.Length; i++)
            {
                model.Vertices[i].Position = Vector3.Transform(model.Vertices[i].Position, Rotation);
                model.Vertices[i].Position += Offset;
            }
        }
        vertexBuffer.Add(new VertexBuffer(graphics, typeof(VertexPositionColorNormalTexture), model.Vertices.Length, BufferUsage.WriteOnly));
        vertexBuffer[vertexBuffer.Count - 1].SetData<VertexPositionColorNormalTexture>(model.Vertices);

        indexBuffer.Add(new IndexBuffer(graphics, typeof(short), model.Indices.Length, BufferUsage.WriteOnly));
        indexBuffer[indexBuffer.Count - 1].SetData(model.Indices);
    }
    public void SetVertexData(GraphicsDevice graphics, PomModel[] model, Vector3 Offset, Matrix Rotation)
    {
        for (int z = 0; z < model.Length; z++)
        {
            Textures.Add(model[z].texture);
            model[z].Vertices = (VertexPositionColorNormalTexture[])model[z].Vertices.Clone();
            if (Offset != Vector3.Zero)
            {
                for (int i = 0; i < model[z].Vertices.Length; i++)
                {
                    model[z].Vertices[i].Position = Vector3.Transform(model[z].Vertices[i].Position,Rotation);
                    model[z].Vertices[i].Position += Offset;
                }
            }
            vertexBuffer.Add(new VertexBuffer(graphics, typeof(VertexPositionColorNormalTexture), model[z].Vertices.Length, BufferUsage.WriteOnly));
            vertexBuffer[vertexBuffer.Count - 1].SetData<VertexPositionColorNormalTexture>(model[z].Vertices);

            indexBuffer.Add(new IndexBuffer(graphics, typeof(short), model[z].Indices.Length, BufferUsage.WriteOnly));
            indexBuffer[indexBuffer.Count - 1].SetData(model[z].Indices);
        }
    }
    public void SetVertexData(GraphicsDevice graphics, VertexPositionColorNormalTexture[] Vertices, short[] indices, Texture2D texture, Vector3 Offset)
    {
        Textures.Add(texture);
        Vertices = (VertexPositionColorNormalTexture[])Vertices.Clone();
        if(Offset!=Vector3.Zero)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position += Offset;
            }
        }
        vertexBuffer.Add(new VertexBuffer(graphics, typeof(VertexPositionColorNormalTexture), Vertices.Length, BufferUsage.WriteOnly));
        vertexBuffer[vertexBuffer.Count-1].SetData<VertexPositionColorNormalTexture>(Vertices);
        
        indexBuffer.Add(new IndexBuffer(graphics, typeof(short), indices.Length, BufferUsage.WriteOnly));
        indexBuffer[indexBuffer.Count-1].SetData(indices);
    }
}
public class Camera
{
    public Matrix world;
    public Matrix view;
    public Matrix projection;
    public Effect material;
    public float RenderDistance;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Up;
    public Vector3 Forward;
    public Vector3 Right;
    public float FOV;
    public RenderTarget2D Render;
    
    public Camera(ContentManager content, GraphicsDevice device, int width, int height)
    {
        material = content.Load<Effect>("Effect");
        Render = new RenderTarget2D(device, width, height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
    }
    public void RenderWorld(GraphicsDevice graphics, World worldScene, float Aspect)
    {
        world = Matrix.CreateTranslation(-Position);
        Forward = Vector3.Transform(Vector3.Forward,Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(Rotation.Y), MathHelper.ToRadians(Rotation.X), MathHelper.ToRadians(Rotation.Z)));
        Right = -Vector3.Transform(Vector3.Right, Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(Rotation.Y), MathHelper.ToRadians(Rotation.X), MathHelper.ToRadians(Rotation.Z)));
        Up = Vector3.Transform(Vector3.Up, Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(Rotation.Y), MathHelper.ToRadians(Rotation.X), MathHelper.ToRadians(Rotation.Z)));
        view = Matrix.CreateLookAt(Vector3.Zero,Forward, Vector3.Up);
        projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Aspect, 0.01f, RenderDistance);
        material.Parameters[0].SetValue(world);
        material.Parameters[1].SetValue(view);
        material.Parameters[2].SetValue(projection);

        graphics.SetRenderTarget(Render);
        graphics.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
        graphics.Clear(worldScene.SkyColor);
        for (int i = 0; i < worldScene.vertexBuffer.Count; i++)
        {
            graphics.SetVertexBuffer(worldScene.vertexBuffer[i]);
            graphics.Indices = worldScene.indexBuffer[i];

            material.Parameters[3].SetValue(worldScene.Textures[i]);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullClockwiseFace;
            graphics.RasterizerState = rasterizerState;

            foreach (EffectPass pass in material.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, worldScene.indexBuffer[0].IndexCount / 3);
            }
        }
        graphics.SetRenderTarget(null);
    }
}
public class Engine : Game
{
    public static GraphicsDeviceManager _graphics;
    public static GraphicsDevice device;
    public static Texture2D NullTexture;
    public static SpriteBatch _spriteBatch;
    public static PomModel Axis;
    public GameManager manager;
    public Engine()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        
        base.Initialize();
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        device = GraphicsDevice;
        NullTexture = new Texture2D(GraphicsDevice,1,1);
        NullTexture.SetData<Color>(new Color[1] { Color.White});
        Axis = PomModel.FromFile(File.ReadAllText("Resources/Axis.obj"), NullTexture);
        manager = new GameManager();
        manager.Init(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        manager.Update(gameTime);

        base.Update(gameTime);
    }
    public static void RenderToScreen(GraphicsDevice device, World world, Camera camera)
    {
        camera.RenderWorld(device, world, (float)_graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight);
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null);
        _spriteBatch.Draw(camera.Render, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
        _spriteBatch.End();
    }
    protected override void Draw(GameTime gameTime)
    {
        manager.Draw();
        base.Draw(gameTime);
    }
}
