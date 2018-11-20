/*
Function: 		Store, update, and draw all visible objects
Author: 		NMCG
Version:		1.1
Date Updated:	
Bugs:			None
Fixes:			None
Mods:           Added support for opaque and semi-transparent rendering
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class ObjectManager : PausableDrawableGameComponent
    {
        #region Fields
        private CameraManager cameraManager;
        private List<DrawnActor3D> removeList, opaqueDrawList, transparentDrawList;
        private RasterizerState rasterizerStateOpaque;
        private RasterizerState rasterizerStateTransparent;
        #endregion

        #region Properties   
        public List<DrawnActor3D> OpaqueDrawList
        {
            get
            {
                return this.opaqueDrawList;
            }
        }
        public List<DrawnActor3D> TransparentDrawList
        {
            get
            {
                return this.transparentDrawList;
            }
        }
        #endregion

        //creates an ObjectManager with lists at initialSize == 20
        public ObjectManager(Game game, CameraManager cameraManager, 
            EventDispatcher eventDispatcher, StatusType statusType)
            : this(game, cameraManager, 10, 10, eventDispatcher, statusType)
        {

        }

        public ObjectManager(Game game, CameraManager cameraManager,
                        int transparentInitialSize, int opaqueInitialSize, 
                        EventDispatcher eventDispatcher, StatusType statusType)
            : base(game, eventDispatcher, statusType)
        {
            this.cameraManager = cameraManager;

            //create two lists - opaque and transparent
            this.opaqueDrawList = new List<DrawnActor3D>(opaqueInitialSize);
            this.transparentDrawList = new List<DrawnActor3D>(transparentInitialSize);
            //create list to store objects to be removed at start of each update
            this.removeList = new List<DrawnActor3D>(0);

            //set up graphic settings
            InitializeGraphics();

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        #region Event Handling
        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.OpacityChanged += EventDispatcher_OpacityChanged;
            eventDispatcher.RemoveActorChanged += EventDispatcher_RemoveActorChanged;
            eventDispatcher.AddActorChanged += EventDispatcher_AddActorChanged;

            //dont forget to call the base method to register for OnStart, OnPause events!
            base.RegisterForEventHandling(eventDispatcher);
        }

        private void EventDispatcher_AddActorChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnAddActor)
            {
                DrawnActor3D actor = eventData.Sender as DrawnActor3D;
                if (actor != null)
                    this.Add(actor);
            }
        }

        private void EventDispatcher_RemoveActorChanged(EventData eventData)
        {

            if (eventData.EventType == EventActionType.OnRemoveActor)
            {
                DrawnActor3D actor = eventData.Sender as DrawnActor3D;
                if (actor != null)
                    this.Remove(actor);
            }
        }

        private void EventDispatcher_OpacityChanged(EventData eventData)
        {
            DrawnActor3D actor = eventData.Sender as DrawnActor3D;

            if (actor != null)
            { 
                if (eventData.EventType == EventActionType.OnOpaqueToTransparent)
                {
                    //remove from opaque and add to transparent
                    this.opaqueDrawList.Remove(actor);
                    this.transparentDrawList.Add(actor);
                }
                else if (eventData.EventType == EventActionType.OnTransparentToOpaque)
                {
                    //remove from transparent and add to opaque
                    this.transparentDrawList.Remove(actor);
                    this.opaqueDrawList.Add(actor);
                }
            }
        }

        #endregion

        private void InitializeGraphics()
        {
            //set the graphics card to repeat the end pixel value for any UV value outside 0-1
            //See http://what-when-how.com/xna-game-studio-4-0-programmingdeveloping-for-windows-phone-7-and-xbox-360/samplerstates-xna-game-studio-4-0-programming/
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Mirror;
            samplerState.AddressV = TextureAddressMode.Mirror;
            Game.GraphicsDevice.SamplerStates[0] = samplerState;

            //opaque objects
            this.rasterizerStateOpaque = new RasterizerState();
            this.rasterizerStateOpaque.CullMode = CullMode.CullCounterClockwiseFace;

            //transparent objects
            this.rasterizerStateTransparent = new RasterizerState();
            this.rasterizerStateTransparent.CullMode = CullMode.None;

        }
        private void SetGraphicsStateObjects(bool isOpaque)
        {
            //Remember this code from our initial aliasing problems with the Sky box?
            //enable anti-aliasing along the edges of the quad i.e. to remove jagged edges to the primitive
            Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            if (isOpaque)
            {
                //set the appropriate state for opaque objects
                Game.GraphicsDevice.RasterizerState = this.rasterizerStateOpaque;

                //disable to see what happens when we disable depth buffering - look at the boxes
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else //semi-transparent
            {
                //set the appropriate state for transparent objects
                Game.GraphicsDevice.RasterizerState = this.rasterizerStateTransparent;

                //enable alpha blending for transparent objects i.e. trees
                Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                //disable to see what happens when we disable depth buffering - look at the boxes
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            }
        }

        public void Add(DrawnActor3D actor)
        {
            if (actor.GetAlpha() == 1)
                this.opaqueDrawList.Add(actor);
            else
                this.transparentDrawList.Add(actor);
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(DrawnActor3D actor)
        {
            this.removeList.Add(actor);
        }

        public DrawnActor3D Find(Predicate<DrawnActor3D> predicate)
        {
            DrawnActor3D drawnActor = null;

            //look in opaque
            drawnActor = this.opaqueDrawList.Find(predicate);
            if (drawnActor != null)
                return drawnActor;

            //look in transparent
            drawnActor = this.transparentDrawList.Find(predicate);
            if (drawnActor != null)
                return drawnActor;

            return null;

        }

        public List<DrawnActor3D> FindAll(Predicate<DrawnActor3D> predicate)
        {
            List<DrawnActor3D> resultList = new List<DrawnActor3D>();

            //look in opaque
            resultList.AddRange(this.opaqueDrawList.FindAll(predicate));
            //look in transparent
            resultList.AddRange(this.transparentDrawList.FindAll(predicate));

            return resultList.Count == 0 ? null : resultList;

        }

        public int Remove(Predicate<DrawnActor3D> predicate)
        {
            List<DrawnActor3D> resultList = null;

            resultList = this.opaqueDrawList.FindAll(predicate);
            if ((resultList != null) && (resultList.Count != 0)) //the actor(s) were found in the opaque list
            {
                foreach (DrawnActor3D actor in resultList)
                    this.removeList.Add(actor);
            }
            else //the actor(s) were found in the transparent list
            {
                resultList = this.transparentDrawList.FindAll(predicate);

                if ((resultList != null) && (resultList.Count != 0))
                    foreach (DrawnActor3D actor in resultList)
                        this.removeList.Add(actor);
            }

            //returns how many objects will be removed in the next update() call
            return removeList != null ? removeList.Count : 0;
        }

        public void Clear()
        {
            this.opaqueDrawList.Clear();
            this.transparentDrawList.Clear();
            this.removeList.Clear();
        }



        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (DrawnActor3D actor in this.removeList)
            {
                if (actor.GetAlpha() == 1)
                    this.opaqueDrawList.Remove(actor);
                else
                    this.transparentDrawList.Remove(actor);
            }

            this.removeList.Clear();
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //remove any outstanding objects since the last update
            ApplyRemove();

            //update all your opaque objects
            foreach (DrawnActor3D actor in this.opaqueDrawList)
            {
                if ((actor.GetStatusType() & StatusType.Update) == StatusType.Update) //if update flag is set
                    actor.Update(gameTime);
            }

            //update all your transparent objects
            foreach (DrawnActor3D actor in this.transparentDrawList)
            {
                if ((actor.GetStatusType() & StatusType.Update) == StatusType.Update) //if update flag is set
                {
                    actor.Update(gameTime);
                    //used to sort objects by distance from the camera so that proper depth representation will be shown
                    MathUtility.SetDistanceFromCamera(actor as Actor3D, this.cameraManager.ActiveCamera);
                }
            }

            //sort so that the transparent objects closest to the camera are the LAST transparent objects drawn
            SortTransparentByDistance();

            base.ApplyUpdate(gameTime);
        }
       

        private void SortTransparentByDistance()
        {
            //sorting in descending order
            this.transparentDrawList.Sort((a, b) => (b.Transform.DistanceToCamera.CompareTo(a.Transform.DistanceToCamera)));
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            //draw the scene for all of the cameras in the cameramanager
            foreach (Camera3D activeCamera in this.cameraManager)
            {
                //set the viewport dimensions to the size defined by the active camera
                Game.GraphicsDevice.Viewport = activeCamera.Viewport;

                //set the gfx to render opaque objects
                SetGraphicsStateObjects(true);
                foreach (DrawnActor3D actor in this.opaqueDrawList)
                {
                    DrawActor(gameTime, actor, activeCamera);
                }

                //set the gfx to render semi-transparent objects
                SetGraphicsStateObjects(false);
                foreach (DrawnActor3D actor in this.transparentDrawList)
                {
                    DrawActor(gameTime, actor, activeCamera);
                }
            }
            base.ApplyDraw(gameTime);
        }

        //calls the DrawObject() based on underlying object type
        private void DrawActor(GameTime gameTime, DrawnActor3D actor, Camera3D activeCamera)
        {
            //was the drawn enum value set?
            if ((actor.StatusType & StatusType.Drawn) == StatusType.Drawn)
            {
                if (actor is ModelObject)
                {
                    DrawObject(gameTime, actor as ModelObject, activeCamera);
                }          
                //we will add additional else...if statements here to render other object types (e.g model, animated, billboard etc)
            }
        }

        //draw a model object 
        private void DrawObject(GameTime gameTime, ModelObject modelObject, Camera3D activeCamera)
        {
            if (modelObject.Model != null)
            {
                modelObject.EffectParameters.SetParameters(activeCamera);
                foreach (ModelMesh mesh in modelObject.Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = modelObject.EffectParameters.Effect;
                    }
                    modelObject.EffectParameters.SetWorld(modelObject.BoneTransforms[mesh.ParentBone.Index] * modelObject.GetWorldMatrix());
                    mesh.Draw();
                }
            }
        }
    }
}
