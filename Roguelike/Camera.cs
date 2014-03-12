using Microsoft.Xna.Framework;
using System;

namespace Roguelike
{
    internal class Camera
    {
        public Point position;
        private Controller controller;
        private int tileSize;
        private Map map;
        private GraphicsDeviceManager graphics;
        public float scale, prevScale;
        private int maxTilesX, maxTilesY;

        private const float MARGINSIZECONST = 2.5f; //must be larger than 2.0 to prevent jittering

        public Camera(Controller controller, Point position, int tileSize)
        {
            this.controller = controller;
            this.tileSize = tileSize;
            this.position = position;
            this.graphics = controller.graphics;
            map = controller.map;
            scale = map.scale;
            maxTilesX = (int)Math.Ceiling(((float)graphics.PreferredBackBufferWidth / ((float)tileSize * scale)));
            maxTilesY = (int)Math.Ceiling(((float)graphics.PreferredBackBufferHeight / ((float)tileSize * scale)));
        }

        public void ResetPosition()
        {
            position = controller.player.position;
        }

        private int marginSize;

        public void Update()
        {
            marginSize = map.floorSize / 2 - map.floorSize / 4;
            prevScale = scale;
            scale = map.scale;

            if (prevScale != scale)
            {
                maxTilesX = (int)Math.Round(((float)graphics.PreferredBackBufferWidth / ((float)tileSize * scale)));
                maxTilesY = (int)Math.Round(((float)graphics.PreferredBackBufferHeight / ((float)tileSize * scale)));
            }

            int marginSizeX = (int)(maxTilesX / MARGINSIZECONST);
            int marginSizeY = (int)(maxTilesY / MARGINSIZECONST);

            Point marginLeft = new Point(position.X + marginSizeX, position.Y + marginSizeY);
            Point marginRight = new Point(position.X + maxTilesX - marginSizeX - 1, position.Y + maxTilesY - marginSizeY - 1);

            if (controller.player.position.X < marginLeft.X)
            {
                position.X -= marginLeft.X - controller.player.position.X;
            }

            if (controller.player.position.X + 1 > marginRight.X)
            {
                position.X += controller.player.position.X - marginRight.X;
            }

            if (controller.player.position.Y < marginLeft.Y)
            {
                position.Y -= marginLeft.Y - controller.player.position.Y;
            }

            if (controller.player.position.Y + 1 > marginRight.Y)
            {
                position.Y += controller.player.position.Y - marginRight.Y;
            }
        }

        public Matrix GetTransformMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-position.X * tileSize, -position.Y * tileSize, 0)) * Matrix.CreateScale(scale, scale, 0);
        }
    }
}