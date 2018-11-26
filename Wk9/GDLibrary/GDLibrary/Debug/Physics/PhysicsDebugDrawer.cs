/*
Function: 		Renders the collision skins of any collidable objects within the scene. We can disable this component for the release.
Author: 		NMCG
Version:		1.0
Date Updated:	27/10/17
Bugs:			
Fixes:			None
*/
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace GDLibrary
{
    public class PhysicsDebugDrawer : PausableDrawableGameComponent
    {
        #region Fields
        private CameraManager cameraManager;
        private CameraLayoutType cameraLayoutType;
        private BasicEffect basicEffect;
        private List<VertexPositionColor> vertexData;
        private VertexPositionColor[] wf;
        private ObjectManager objectManager;

        //temp local var
        private IActor actor;
        #endregion
        #region Properties  
        public CameraLayoutType CameraLayoutType
        {
            get
            {
                return this.cameraLayoutType;
            }
            set
            {
                this.cameraLayoutType = value;
            }
        }
        #endregion
        public PhysicsDebugDrawer(Game game, CameraManager cameraManager, ObjectManager objectManager,
            EventDispatcher eventDispatcher, StatusType statusType,
            CameraLayoutType cameraLayoutType)
            : base(game, eventDispatcher, statusType)
        {
            this.cameraManager = cameraManager;
            this.cameraLayoutType = cameraLayoutType;
            this.objectManager = objectManager;
            this.vertexData = new List<VertexPositionColor>();
            this.basicEffect = new BasicEffect(game.GraphicsDevice);
        }

        #region Event Handling
        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.DebugChanged += EventDispatcher_DebugChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        //enable dynamic show/hide of debug info
        private void EventDispatcher_DebugChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnToggle)
            {
                if (this.StatusType == StatusType.Off)
                    this.StatusType = StatusType.Drawn | StatusType.Update;
                else
                    this.StatusType = StatusType.Off;
            }
        }
        #endregion

        protected override void ApplyDraw(GameTime gameTime)
        {
            if (this.cameraLayoutType == CameraLayoutType.Single)
                ApplySingleCameraDraw(gameTime, this.cameraManager.ActiveCamera);
            else
                ApplyMultiCameraDraw(gameTime);
        }

        private void ApplyMultiCameraDraw(GameTime gameTime)
        {
            //add the vertices for each and every drawn object (opaque or transparent) to the vertexData array for drawing
            ProcessAllDrawnObjects();

            //no vertices to draw - would happen if we forget to call DrawCollisionSkins() above or there were no drawn objects to see!
            if (vertexData.Count == 0) return;

            this.basicEffect.AmbientLightColor = Vector3.One;
            this.basicEffect.VertexColorEnabled = true;

            foreach (Camera3D camera in this.cameraManager)
                DrawCollisionSkin(camera);

            vertexData.Clear();
        }

        private void ApplySingleCameraDraw(GameTime gameTime, Camera3D activeCamera)
        {
            //add the vertices for each and every drawn object (opaque or transparent) to the vertexData array for drawing
            ProcessAllDrawnObjects();

            //no vertices to draw - would happen if we forget to call DrawCollisionSkins() above or there were no drawn objects to see!
            if (vertexData.Count == 0) return;

            this.basicEffect.AmbientLightColor = Vector3.One;
            this.basicEffect.VertexColorEnabled = true;

            DrawCollisionSkin(activeCamera);

            vertexData.Clear();
        }

        private void DrawCollisionSkin(Camera3D activeCamera)
        {
            this.Game.GraphicsDevice.Viewport = activeCamera.Viewport;
            this.basicEffect.View = activeCamera.View;
            this.basicEffect.Projection = activeCamera.ProjectionParameters.Projection;
            this.basicEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertexData.ToArray(), 0, vertexData.Count - 1);
        }

        //debug method to draw collision skins for collidable objects and zone objects
        private void ProcessAllDrawnObjects()
        {
            for (int i = 0; i < objectManager.OpaqueDrawList.Count; i++)
            {
                actor = objectManager.OpaqueDrawList[i];
                if (actor is CollidableObject)
                {
                    AddCollisionSkinVertexData(actor as CollidableObject);
                }
            }

            for (int i = 0; i < objectManager.TransparentDrawList.Count; i++)
            {
                actor = objectManager.TransparentDrawList[i];
                if (actor is CollidableObject)
                {
                    AddCollisionSkinVertexData(actor as CollidableObject);
                }
            }
        }

        public void AddVertexDataForShape(List<Vector3> shape, Color color)
        {
            if (vertexData.Count > 0)
            {
                Vector3 v = vertexData[vertexData.Count - 1].Position;
                vertexData.Add(new VertexPositionColor(v, color));
                vertexData.Add(new VertexPositionColor(shape[0], color));
            }

            foreach (Vector3 p in shape)
            {
                vertexData.Add(new VertexPositionColor(p, color));
            }
        }

        public void AddVertexDataForShape(List<Vector3> shape, Color color, bool closed)
        {
            AddVertexDataForShape(shape, color);

            Vector3 v = shape[0];
            vertexData.Add(new VertexPositionColor(v, color));
        }

        public void AddVertexDataForShape(List<VertexPositionColor> shape, Color color)
        {
            if (vertexData.Count > 0)
            {
                Vector3 v = vertexData[vertexData.Count - 1].Position;
                vertexData.Add(new VertexPositionColor(v, color));
                vertexData.Add(new VertexPositionColor(shape[0].Position, color));
            }

            foreach (VertexPositionColor vps in shape)
            {
                vertexData.Add(vps);
            }
        }

        public void AddVertexDataForShape(VertexPositionColor[] shape, Color color)
        {
            if (vertexData.Count > 0)
            {
                Vector3 v = vertexData[vertexData.Count - 1].Position;
                vertexData.Add(new VertexPositionColor(v, color));
                vertexData.Add(new VertexPositionColor(shape[0].Position, color));
            }

            foreach (VertexPositionColor vps in shape)
            {
                vertexData.Add(vps);
            }
        }

        public void AddVertexDataForShape(List<VertexPositionColor> shape, Color color, bool closed)
        {
            AddVertexDataForShape(shape, color);

            VertexPositionColor v = shape[0];
            vertexData.Add(v);
        }

        public void AddCollisionSkinVertexData(CollidableObject collidableObject)
        {
            if (!collidableObject.Body.CollisionSkin.GetType().Equals(typeof(JigLibX.Geometry.Plane)))
            {
                wf = collidableObject.Collision.GetLocalSkinWireframe();

                // if the collision skin was also added to the body
                // we have to transform the skin wireframe to the body space
                if (collidableObject.Body.CollisionSkin != null)
                {
                    collidableObject.Body.TransformWireframe(wf);
                }

                AddVertexDataForShape(wf, collidableObject.EffectParameters.DiffuseColor);
            }
        }
    }
}
