using GDLibrary;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GDApp
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        #region Fields
        public string ID = "groundPanel_";
        public int outlinePanelCount = 1;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private ObjectManager object3DManager;
        private KeyboardManager keyboardManager;
        private MouseManager mouseManager;
        private Integer2 resolution;
        private Integer2 screenCentre;
        private InputManagerParameters inputManagerParameters;
        private CameraManager cameraManager;
        private ContentDictionary<Model> modelDictionary;
        private ContentDictionary<Texture2D> textureDictionary;
        private ContentDictionary<SpriteFont> fontDictionary;
        private Dictionary<string, RailParameters> railDictionary;
        private Dictionary<string, Track3D> track3DDictionary;
        private Dictionary<string, EffectParameters> effectDictionary;
        private EventDispatcher eventDispatcher;
        private SoundManager soundManager;
        private MyMenuManager menuManager;
        private PhysicsManager physicsManager;
        private ModelObject drivableBoxObject;
        private PhysicsDebugDrawer physicsDebugDrawer;
        private PickingManager pickingManager;
        private HUDManager hudManager;
        private HeroPlayerObject player;
        private bool shovel = false;
        private bool once = true;
        private bool added = false;
        private bool coat = false;
        private bool gameOver = false;
        private CameraLayoutType cameraLayoutType;

        #endregion

        #region Constructors
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        #endregion

        #region Initialization
        protected override void Initialize()
        {
            //set the title
            Window.Title = "SnowFront";

            this.cameraLayoutType = CameraLayoutType.Single;

            #region Assets & Dictionaries
            InitializeDictionaries();
            #endregion

            #region Graphics Related
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.resolution = ScreenUtility.XVGA;
            this.screenCentre = this.resolution / 2;
            InitializeGraphics();
            InitializeEffects();
            #endregion

            #region Event Handling
            //add the component to handle all system events
            this.eventDispatcher = new EventDispatcher(this, 20);
            Components.Add(this.eventDispatcher);
            #endregion

            #region Assets
            LoadAssets();
            #endregion

            #region Initialize Managers
            //Keyboard
            this.keyboardManager = new KeyboardManager(this);
            Components.Add(this.keyboardManager);

            //CD-CR using JigLibX and add debug drawer to visualise collision skins
            this.physicsManager = new PhysicsManager(this, this.eventDispatcher, StatusType.Off, AppData.BigGravity);
            Components.Add(this.physicsManager);

            //Mouse
            bool bMouseVisible = true;
            this.mouseManager = new MouseManager(this, bMouseVisible, this.physicsManager);
            this.mouseManager.SetPosition(this.screenCentre);
            Components.Add(this.mouseManager);

            //bundle together for easy passing
            this.inputManagerParameters = new InputManagerParameters(this.mouseManager, this.keyboardManager);

            //this is a list that updates all cameras
            this.cameraManager = new CameraManager(this, 5, this.eventDispatcher,
                StatusType.Off);
            Components.Add(this.cameraManager);


            //picking
            //use this predicate anytime we want to decide if a mouse over object is interesting to the PickingManager
            Predicate<CollidableObject> collisionPredicate = new Predicate<CollidableObject>(CollisionUtility.IsCollidableObjectOfInterest);
            //listens for picking with the mouse on valid (based on specified predicate) collidable objects and pushes notification events to listeners
            this.pickingManager = new PickingManager(
                this,
                this.eventDispatcher,
                StatusType.Off,
                this.inputManagerParameters,
                this.cameraManager,
                PickingBehaviourType.PickAndPlace,
                AppData.PickStartDistance,
                AppData.PickEndDistance,
                collisionPredicate);
            Components.Add(this.pickingManager);

            //Object3D
            this.object3DManager = new ObjectManager(this, this.cameraManager,
                this.eventDispatcher, StatusType.Off, this.cameraLayoutType);
            this.object3DManager.DrawOrder = 1;
            Components.Add(this.object3DManager);

            //Sound
            this.soundManager = new SoundManager(this, this.eventDispatcher, StatusType.Update, "Content/Assets/Audio/", "Demo2DSound.xgs", "WaveBank1.xwb", "SoundBank1.xsb");
            Components.Add(this.soundManager);

            //Menu
            this.menuManager = new MyMenuManager(this, this.inputManagerParameters,
                this.cameraManager, this.spriteBatch, this.eventDispatcher,
                StatusType.Drawn | StatusType.Update);
            this.menuManager.DrawOrder = 2;
            Components.Add(this.menuManager);

            this.hudManager = new HUDManager(this, this.spriteBatch, this.eventDispatcher, 10, StatusType.Off);
            this.hudManager.DrawOrder = 3;
            Components.Add(this.hudManager);
            #endregion

            #region Load Game
            //load game happens before cameras are loaded because we may add a third person camera that needs a reference to a loaded Actor
            int worldScale = 1000;
            int gameLevel = 1;
            LoadGame(worldScale, gameLevel);
            #endregion

            #region Cameras
            InitializeCameras(ScreenLayoutType.ThirdPerson);
            #endregion

            #region Menu & UI
            InitializeMenu();
            InitializeUI();
            //since debug needs sprite batch then call here
            #endregion

#if DEBUG
            InitializeDebug(true);
            InitializeDebugCollisionSkinInfo();
#endif

            base.Initialize();
        }
        #endregion

        #region Load Game Content
        //load the contents for the level specified
        private void LoadGame(int worldScale, int gameLevel)
        {
            //remove anything from the last time LoadGame may have been called
            this.object3DManager.Clear();

            //Non - collidable
            InitializeNonCollidableSkyBox(worldScale);
            //Collidable
            InitializeCollidableGround(worldScale);

            //InitializeLevelOutline();

            if (gameLevel == 1)
            {
                //InitializePlayer();
                //demo high vertex count trianglemesh
                InitializeTrees(worldScale);
                InitializeStaticCollidableTriangleMeshObjects();
                InitializeIce();
                InitializeAnimatedPlayer();
                InitializeRoadRoof();
                InitializeIcicle();
                InitializeFallingTree();
                InitializeFallingTreeTrigger();
                InitializeIcicleTrigger();
                InitializeCollisionDialogue();
                InitializeGoal();
                //demo medium and low vertex count trianglemesh
                //InitializeStaticCollidableMediumPolyTriangleMeshObjects();
                //InitializeStaticCollidableLowPolyTriangleMeshObjects();
                //demo dynamic collidable objects with user-defined collision primitives
                //InitializeDynamicCollidableObjects();

                //add level elements
                //InitializeBuildings();
                InitializeWallsFences();
                InitializeRoad();
            }
            else if (gameLevel == 2)
            {
                //add different things for your next level
            }
        }

        private void InitializeAnimatedPlayer()
        {
            Transform3D transform3D = null;

            transform3D = new Transform3D(new Vector3(-750, 25, -340),
               new Vector3(-90, 90, 0),
                0.1f * Vector3.One,
                 Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.DiffuseColor = Color.Beige;
            //no specular, emissive, directional lights
            //if we dont specify a texture then the object manager will draw using whatever textures were baked into the animation in 3DS Max
            effectParameters.Texture = this.textureDictionary["charTexture"];

            this.animatedHeroPlayerObject = new HeroAnimatedPlayerObject("hero",
            ActorType.Player, transform3D,
                effectParameters,
                AppData.CameraMoveKeys,
                4,
                20,
                1,  1,1f //accel, decel
                ,
                AppData.PlayerRotationSpeed,
                AppData.PlayerJumpHeight,
                new Vector3(0, -10, 0), //offset inside capsule
                this.keyboardManager,
                this.eventDispatcher);
            this.animatedHeroPlayerObject.Enable(false, AppData.PlayerMass);

            //add the animations
            string takeName = "Take 001";
            string fileNameNoSuffix = "char_idle";
            this.animatedHeroPlayerObject.AddAnimation(takeName, fileNameNoSuffix, this.modelDictionary[fileNameNoSuffix]);
            fileNameNoSuffix = "char_walk";
            this.animatedHeroPlayerObject.AddAnimation(takeName, fileNameNoSuffix, this.modelDictionary[fileNameNoSuffix]);
            fileNameNoSuffix = "char_fall";
            this.animatedHeroPlayerObject.AddAnimation(takeName, fileNameNoSuffix, this.modelDictionary[fileNameNoSuffix]);


            //set the start animtion
            this.animatedHeroPlayerObject.SetAnimation("Take 001", "char_idle");
            this.animatedHeroPlayerObject.AttachController(new SlipController("sc1",
                ControllerType.Slip,
                PlayStatusType.Stop,
                this.eventDispatcher));
            this.object3DManager.Add(animatedHeroPlayerObject);

        }

        private void InitializeGoal()
        {
            Transform3D transform3D = new Transform3D(new Vector3(820, 0, -320),
              new Vector3(0, 0 , 0), 2 * new Vector3(1.5f, 1, 1), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.DiffuseColor = Color.White;
            effectParameters.Texture = this.textureDictionary["house-low-texture"];
            Model model = this.modelDictionary["house"];

            CollidableObject goal = new TriangleMeshObject("goal", ActorType.Goal, 
                transform3D, effectParameters, model, new MaterialProperties(0.3f, 0.7f, 0.5f));
            goal.Enable(true, 1);
            this.object3DManager.Add(goal);
            
        }

        private void InitializeCollisionDialogue()
        {
            //Creating the effect for the collidableobject model
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            //effectParameters.Texture = this.textureDictionary["ml"];
            effectParameters.DiffuseColor = Color.White;

            Vector3 rot = new Vector3(0, 0, 0);
            Vector3 scale = new Vector3(0.4f, 0.16f, 0.4f);
            Vector3 scale2 = new Vector3(0.4f, 0.16f, 0.4f);
            //Creating the transforms  for each of the models
            Transform3D transform1 = new Transform3D(new Vector3(-740, 5, -320), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform2 = new Transform3D(new Vector3(-170, 5, -325), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3 = new Transform3D(new Vector3(-370, 5, -325), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform4 = new Transform3D(new Vector3(-80, 5, -125), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform5 = new Transform3D(new Vector3(10, 5, -475), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform6 = new Transform3D(new Vector3(290, 5, -475), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform7 = new Transform3D(new Vector3(380, 5, -180), rot, scale, Vector3.UnitX, Vector3.UnitY);

            SnowDriftWarning sdw = new SnowDriftWarning("sdw",
               ActorType.Zone,
               transform1,
               effectParameters, this.modelDictionary["box"]);
            sdw.StatusType = StatusType.Update;
            sdw.AddPrimitive(new Sphere(transform1.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdw.Enable(true, 1);

            //remove as it breaks the shovel
            this.object3DManager.Add(sdw);


            TreeWarning tw = new TreeWarning("tw",
               ActorType.Zone,
               transform2,
               effectParameters, this.modelDictionary["box"]);
            tw.StatusType = StatusType.Update;
            tw.AddPrimitive(new Sphere(transform2.Translation, 92), new MaterialProperties(0.2f, 0.8f, 0.7f));
            tw.Enable(true, 1);
            //remove as it breaks the shovel
            this.object3DManager.Add(tw);



            IceWarning iw = new IceWarning("iw",
               ActorType.Zone,
               transform3,
               effectParameters, this.modelDictionary["box"], this.soundManager);
            iw.StatusType = StatusType.Update;

            iw.AddPrimitive(new Sphere(transform3.Translation, 72), new MaterialProperties(0.2f, 0.8f, 0.7f));
            iw.Enable(true, 1);
            //remove as it breaks the shovel
            this.object3DManager.Add(iw);



            ElectricPoleWarning epw = new ElectricPoleWarning("epw",
               ActorType.Zone,
               transform4,
               effectParameters, this.modelDictionary["box"]);
            epw.StatusType = StatusType.Update;
            epw.AddPrimitive(new Sphere(transform4.Translation, 72), new MaterialProperties(0.2f, 0.8f, 0.7f));
            epw.Enable(true, 1);
            //remove as it breaks the shovel
            this.object3DManager.Add(epw);

            ElectricPoleWarning epw2 = new ElectricPoleWarning("epw2",
              ActorType.Zone,
              transform5,
              effectParameters, this.modelDictionary["box"]);
            epw2.StatusType = StatusType.Update;
            epw2.AddPrimitive(new Sphere(transform5.Translation, 72), new MaterialProperties(0.2f, 0.8f, 0.7f));
            epw2.Enable(true, 1);
            //remove as it breaks the shovel
            this.object3DManager.Add(epw2);


            SlipWarning sw = new SlipWarning("sw",
            ActorType.Zone,
            transform6,
            effectParameters, this.modelDictionary["box"]);
            sw.StatusType = StatusType.Update;
            sw.AddPrimitive(new Sphere(transform5.Translation, 72), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sw.Enable(true, 1);
            //remove as it breaks the shovel
            this.object3DManager.Add(sw);


            TooColdWarning tcw = new TooColdWarning("tcw",
           ActorType.Zone,
           transform7,
           effectParameters, this.modelDictionary["box"]);
            tcw.StatusType = StatusType.Update;
            tcw.AddPrimitive(new Sphere(transform5.Translation, 102), new MaterialProperties(0.2f, 0.8f, 0.7f));
            tcw.Enable(true, 1);
            //remove as it breaks the shovel
            this.object3DManager.Add(tcw);

        }

        private void InitializeFallingTreeTrigger()
        {

            Transform3D transform3D = new Transform3D(new Vector3(-300, 20, -400),
                new Vector3(0, 0, 0), 2 * new Vector3(1.5f, 1, 2f), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.DiffuseColor = Color.White;
            //Model model = this.modelDictionary["snowDrift"];
            TreeZone tz = new TreeZone("tz1",
               ActorType.Zone,
               transform3D,
               effectParameters,
               this.modelDictionary["box"]);
            tz.StatusType = StatusType.Update;
            tz.AddPrimitive(new Box(tz.Transform.Translation, Matrix.Identity, 12 * 2.54f * new Vector3(6, 1, 6)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            tz.Enable(true, 1);
            tz.AttachController(new TreeFallingController("tc1", ControllerType.TreeZone, PlayStatusType.Stop, this.eventDispatcher));
            this.object3DManager.Add(tz);
        }

        private void InitializeFallingTree()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["tree"];
            effectParameters.DiffuseColor = Color.White;
            Transform3D transform3DFallingTree = new Transform3D(new Vector3(-85, 130, -565), new Vector3(0, 0, 0), new Vector3(0.5f, 0.8f, 0.5f), Vector3.UnitX, Vector3.UnitY);


            TreeObject FallingTree = new TreeObject("Falling Tree",ActorType.FallingTree,transform3DFallingTree,
              effectParameters, this.modelDictionary["tree_centred"],this.eventDispatcher);

            //FallingTree.AddPrimitive(new Box(FallingTree.Transform.Translation + new Vector3(0, 100, 0), Matrix.Identity,5*2.54f*new Vector3(2,5,2))
            //    , new MaterialProperties(0.2f, 0.2f, 1.7f));0
            
            FallingTree.AddPrimitive(new Box(FallingTree.Transform.Translation, Matrix.Identity,  2 * 2.54f * new Vector3(5, 40, 5))
                , new MaterialProperties(0.2f, 0.2f, 1.7f));

            FallingTree.Enable(true, 10);

            FallingTree.AttachController(new EnablePhysicsController("epc", ControllerType.EnablePhysics, PlayStatusType.Stop));
            this.object3DManager.Add(FallingTree);

        }

        private void InitializeIcicleTrigger()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-70, 20, -100),
                new Vector3(0, 0, 0), 2 * new Vector3(1.5f, 1, 1), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.DiffuseColor = Color.White;
            //Model model = this.modelDictionary["snowDrift"];
            IcicleZone iz = new IcicleZone("iz1",
               ActorType.Zone,
               transform3D,
               effectParameters,
               this.modelDictionary["box"]);
            iz.StatusType = StatusType.Update;
            iz.AddPrimitive(new Box(iz.Transform.Translation, Matrix.Identity, 12 * 2.54f * new Vector3(6, 1, 4)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            iz.Enable(true, 1);
            iz.AttachController(new IcicleController("ic1", ControllerType.TreeZone, PlayStatusType.Stop, this.eventDispatcher));
            this.object3DManager.Add(iz);
        }

        private void InitializeIcicle()
        {
            CollidableObject collidableObject = null;
            int count = 0;
            Transform3D transform3D = new Transform3D(new Vector3(-155, -4, -100),
                 new Vector3(0, 0, 0), 2 * new Vector3(1.5f, 1, 1), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["iceSheet"];
            Model model = this.modelDictionary["icycle"];
            IcicleObject archetypeCollidableObject = new IcicleObject("icicle - ", ActorType.Icicle, Transform3D.Zero, effectParameters, model);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    collidableObject = (IcicleObject)archetypeCollidableObject.Clone();
                    collidableObject.ID += count;
                    count++;
                    Console.WriteLine(collidableObject.ID);
                    collidableObject.Transform = new Transform3D(new Vector3(-110 + 60 * i, 40, -120 + 30 * j), new Vector3(0, 0, 0), 0.7f * new Vector3(1, 1.2f, 1), Vector3.UnitX, Vector3.UnitY);
                    collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, /*important do not change - cm to inch*/3 * 2.54f * new Vector3(1, 2.5f, 1)), new MaterialProperties(0f, 0.8f, 0.7f));

                    //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                    collidableObject.Enable(true, 10);
                    collidableObject.AttachController(new EnablePhysicsController("epc " + count, ControllerType.EnablePhysics, PlayStatusType.Stop));

                    this.object3DManager.Add(collidableObject);
                }
            }

            // collidableObject.Enable(true, 1);
            // this.object3DManager.Add(collidableObject);
        }

        private void InitializeRoadRoof()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-50, -4, -80),
                 new Vector3(0, 0, 0), 0.5f * new Vector3(0.7f, 0.7f, 1.3f), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["crate1"];

            ModelObject roadRoof = new ModelObject("road roof", ActorType.Prop, transform3D, effectParameters, this.modelDictionary["RoadRoof"]);
            this.object3DManager.Add(roadRoof);
        }

        private void InitializeIce()
        {
            //middle ice
            Transform3D transform3D = new Transform3D(new Vector3(-155, -4, -300),
                 new Vector3(0, 0, 0), 2 * new Vector3(1.5f, 1, 1), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["iceSheet"];
            effectParameters.SpecularPower = 100;
            CollidableObject collidableObject = new TriangleMeshObject("ice", ActorType.Ice, transform3D,
                            effectParameters, this.modelDictionary["road"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);


            //left far back ice
            transform3D = new Transform3D(new Vector3(530, -4, -410),
                new Vector3(0, 0, 0), 2 * new Vector3(2.2f, 1, 1.5f), Vector3.UnitX, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("ice", ActorType.Ice, transform3D,
                            effectParameters, this.modelDictionary["road"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right far back ice
            transform3D = new Transform3D(new Vector3(580, -4, -150),
               new Vector3(0, 0, 0), 2 * new Vector3(1.3f, 1, 1), Vector3.UnitX, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("ice", ActorType.Ice, transform3D,
                            effectParameters, this.modelDictionary["road"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right middle ice
            transform3D = new Transform3D(new Vector3(280, -4, -150),
               new Vector3(0, 0, 0), 2 * new Vector3(1.3f, 1, 1), Vector3.UnitX, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("ice", ActorType.Ice, transform3D,
                            effectParameters, this.modelDictionary["road"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

        }

        private void InitializePlayer()
        {
            Transform3D transform = new Transform3D(
                new Vector3(-750, 25, -340),
                new Vector3(-90, 90, 0),
                0.1f * Vector3.One,
                Vector3.UnitX,
                Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["charProfileFinal"];

            Model model = this.modelDictionary["Character_model_1"];


            //CollidableObject test = new CollidableObject("aa", ActorType.Player, transform, effectParameters, model);
            this.player = new HeroPlayerObject("hpo1",
                ActorType.Player,
                transform,
                effectParameters,
                model,
                AppData.CameraMoveKeys,
                4, 20, 1, 1, 10,
                new Vector3(0, -10, 0),
                this.keyboardManager,
                this.eventDispatcher);
            this.player.Enable(false, 100);

            this.player.AttachController(new SlipController("sc1",
                ControllerType.Slip,
                PlayStatusType.Stop,
                this.eventDispatcher));

            this.object3DManager.Add(this.player);
        }

        //skybox is a non-collidable series of ModelObjects with no lighting
        private void InitializeNonCollidableSkyBox(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale));

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];

            //create a archetype to use for cloning
            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["plane1"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            #region Skybox
            //add the back skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["back"];
            //rotate the default plane 90 degrees around the X-axis (use the thumb and curled fingers of your right hand to determine +ve or -ve rotation value)
            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);

            /*
             * Move the plane back to meet with the back edge of the grass (by based on the original 3DS Max model scale)
             * Note:
             * - the interaction between 3DS Max and XNA units which result in the scale factor used below (i.e. 1 x 2.54 x worldScale)/2
             * - that I move the plane down a little on the Y-axiz, purely for aesthetic purposes
             */
            clonePlane.Transform.Translation = new Vector3(0, 0, (-2.54f * worldScale) / 2.0f);
            this.object3DManager.Add(clonePlane);

            //As an exercise the student should add the remaining 4 skybox planes here by repeating the clone, texture assignment, rotation, and translation steps above...
            //add the left skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["left"];
            clonePlane.Transform.Rotation = new Vector3(90, 90, 0);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.0f, 0, 0);
            this.object3DManager.Add(clonePlane);

            //add the right skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["right"];
            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3((2.54f * worldScale) / 2.0f, 0, 0);
            this.object3DManager.Add(clonePlane);

            //add the top skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["sky"];
            //notice the combination of rotations to correctly align the sky texture with the sides
            clonePlane.Transform.Rotation = new Vector3(180, -90, 0);
            clonePlane.Transform.Translation = new Vector3(0, ((2.54f * worldScale) / 2.0f), 0);
            this.object3DManager.Add(clonePlane);

            //add the front skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["front"];
            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3(0, 0, (2.54f * worldScale) / 2.0f);
            this.object3DManager.Add(clonePlane);
            #endregion
        }

        //tree is a non-collidable ModelObject (i.e. in final game the player wont ever reach the far-distance tree) with no-lighting
        private void InitializeNonCollidableFoliage(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the decorator elements (e.g. trees etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale));

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];
            //a fix to ensure that any image containing transparent pixels will be sent to the correct draw list in ObjectManager
            effectParameters.Alpha = 0.99f;

            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["plane1"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            //tree
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["tree2"];
            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);
            /*
             * ISRoT - Scale operations are applied before rotation in XNA so to make the tree tall (i.e. 10) we actually scale 
             * along the Z-axis (remember the original plane is flat on the XZ axis) and then flip the plane to stand upright.
             */
            clonePlane.Transform.Scale = new Vector3(5, 1, 10);
            //y-displacement is (10(XNA) x 2.54f(3DS Max))/2 = 12.7f
            clonePlane.Transform.Translation = new Vector3(0, ((clonePlane.Transform.Scale.Z * 2.54f) / 2), -20);
            this.object3DManager.Add(clonePlane);
        }

        //the ground is simply a large flat box with a Box primitive collision surface attached
        private void InitializeCollidableGround(int worldScale)
        {
            CollidableObject collidableObject = null;
            Transform3D transform3D = null;

            /*
             * Note that if we use DualTextureEffectParameters then (a) we must create a model (i.e. box2.fbx) in 3DS Max with two texture channels (i.e. use Unwrap UVW twice)
             * because each texture (diffuse and lightmap) requires a separate set of UV texture coordinates, and (b), this effect does NOT allow us to set up lighting. 
             * Why? Well, we don't need lighting because we can bake a static lighting response into the second texture (the lightmap) in 3DS Max).
             * 
             * See https://knowledge.autodesk.com/support/3ds-max/learn-explore/caas/CloudHelp/cloudhelp/2016/ENU/3DSMax/files/GUID-37414F9F-5E33-4B1C-A77F-547D0B6F511A-htm.html
             * See https://www.youtube.com/watch?v=vuHdnxkXpYo&t=453s
             * See https://www.youtube.com/watch?v=AqiNpRmENIQ&t=1892s
             * 
             */
            Model model = this.modelDictionary["box2"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["snow"];
            effectParameters.DiffuseColor = Color.White;

            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, new Vector3(worldScale, 0.001f, worldScale), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, effectParameters, model);
            collidableObject.AddPrimitive(new JigLibX.Geometry.Plane(transform3D.Up, transform3D.Translation), new MaterialProperties(0.8f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1); //change to false, see what happens.
            this.object3DManager.Add(collidableObject);
        }

        private void InitializeStaticCollidableSnowDrift()
        {
            //Creating the effect for the collidableobject model
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["snow"];
            effectParameters.SpecularPower = 10;
            effectParameters.DiffuseColor = Color.White;

            Vector3 rot = new Vector3(0, 0, 0);
            Vector3 scale = new Vector3(0.4f, 0.16f, 0.4f);
            //Creating the transforms  for each of the models
            Transform3D transform1 = new Transform3D(new Vector3(-660, 5, -280), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform2 = new Transform3D(new Vector3(-660, 5, -350), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3 = new Transform3D(new Vector3(-140, 5, -250), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform4 = new Transform3D(new Vector3(60, 5, -220), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform5 = new Transform3D(new Vector3(120, 5, -540), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform6 = new Transform3D(new Vector3(420, 5, -220), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform7 = new Transform3D(new Vector3(520, 5, -220), rot, scale, Vector3.UnitX, Vector3.UnitY);

            Transform3D transform8 = new Transform3D(new Vector3(420, 5, -520), rot, scale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform9 = new Transform3D(new Vector3(560, 5, -520), rot, scale, Vector3.UnitX, Vector3.UnitY);

            //creating the collidable models

            SnowDriftZone sdz = new SnowDriftZone("sdz",
                ActorType.Snow,
                transform1,
                effectParameters,
                this.modelDictionary["snow_drift"]);

            sdz.AddPrimitive(new Sphere(transform1.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz.Enable(true, 1);

            this.object3DManager.Add(sdz);

            SnowDriftZone sdz2 = new SnowDriftZone("sdz2",
                ActorType.Snow,
                transform2,
                effectParameters,
                this.modelDictionary["snow_drift"]);

            sdz2.AddPrimitive(new Sphere(transform2.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz2.Enable(true, 1);

            this.object3DManager.Add(sdz2);


            SnowDriftZone sdz3 = new SnowDriftZone("sdz3",
                ActorType.Snow,
                transform3,
                effectParameters,
                this.modelDictionary["snow_drift"]);
            sdz3.AddPrimitive(new Sphere(transform3.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz3.Enable(true, 1);
            this.object3DManager.Add(sdz3);

            SnowDriftZone sdz4 = new SnowDriftZone("sdz4",
                ActorType.Snow,
                transform4,
                effectParameters,
                this.modelDictionary["snow_drift"]);
            sdz4.AddPrimitive(new Sphere(transform4.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz4.Enable(true, 1);
            this.object3DManager.Add(sdz4);

            SnowDriftZone sdz5 = new SnowDriftZone("sdz5",
                ActorType.Snow,
                transform5,
                effectParameters,
                this.modelDictionary["snow_drift"]);

            sdz5.AddPrimitive(new Sphere(transform5.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz5.Enable(true, 1);
            this.object3DManager.Add(sdz5);

            SnowDriftZone sdz6 = new SnowDriftZone("sdz6",ActorType.Snow,transform6,effectParameters,this.modelDictionary["snow_drift"]);
            sdz6.AddPrimitive(new Sphere(transform6.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz6.Enable(true, 1);
            this.object3DManager.Add(sdz6);

            SnowDriftZone sdz7 = new SnowDriftZone("sdz7",ActorType.Snow,transform7,effectParameters,this.modelDictionary["snow_drift"]);
            sdz7.AddPrimitive(new Sphere(transform7.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz7.Enable(true, 1);
            this.object3DManager.Add(sdz7);

            SnowDriftZone sdz8 = new SnowDriftZone("sdz8",ActorType.Snow,transform8,effectParameters,this.modelDictionary["snow_drift"]);
            sdz8.AddPrimitive(new Sphere(transform8.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz8.Enable(true, 1);
            this.object3DManager.Add(sdz8);

            SnowDriftZone sdz9 = new SnowDriftZone("sdz9", ActorType.Snow,transform9,effectParameters,this.modelDictionary["snow_drift"]);
            sdz9.AddPrimitive(new Sphere(transform9.Translation, 40), new MaterialProperties(0.2f, 0.8f, 0.7f));
            sdz9.Enable(true, 1);
            this.object3DManager.Add(sdz9);

        }

        private void InitializeStaticCollidableFallenTree()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            
            effectParameters.Texture = this.textureDictionary["tree"];
            effectParameters.DiffuseColor = Color.White;

            Vector3 treeScale =new Vector3(0.25f,0.5f,0.3f);
            Transform3D transform3DFallenTree = new Transform3D(new Vector3(-75, 10, -260), new Vector3(90, 0, -90),treeScale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3DFallenTree2 = new Transform3D(new Vector3(-270, 10, -240), new Vector3(0, 0, -90), treeScale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3DFallenTree3 = new Transform3D(new Vector3(200, 10, -320), new Vector3(-40, 0, -90),treeScale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3DFallenTree4 = new Transform3D(new Vector3(400, 10, -320), new Vector3(-140, 0, -90),treeScale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3DFallenTree5 = new Transform3D(new Vector3(340, 10, -490), new Vector3(-0, 0, -90), treeScale, Vector3.UnitX, Vector3.UnitY);
            Transform3D transform3DFallenTree6 = new Transform3D(new Vector3(590, 10, -490), new Vector3(-180, 0, -90), treeScale, Vector3.UnitX, Vector3.UnitY);




            CollidableObject FallenTree1 = new TriangleMeshObject("fallen tree", ActorType.CollidableProp, transform3DFallenTree, effectParameters,
             this.modelDictionary["fallenTree"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            FallenTree1.Enable(true, 1);

            CollidableObject FallenTree2 = new TriangleMeshObject("fallen tree 2", ActorType.CollidableProp, transform3DFallenTree2, effectParameters,
            this.modelDictionary["fallenTree"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            FallenTree2.Enable(true, 1);

            CollidableObject FallenTree3 = new TriangleMeshObject("fallen tree 3", ActorType.CollidableProp, transform3DFallenTree3, effectParameters,
            this.modelDictionary["fallenTree"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            FallenTree3.Enable(true, 1);

            CollidableObject FallenTree4 = new TriangleMeshObject("fallen tree 4", ActorType.CollidableProp, transform3DFallenTree4, effectParameters,
            this.modelDictionary["fallenTree"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            FallenTree4.Enable(true, 1);

            CollidableObject FallenTree5 = new TriangleMeshObject("fallen tree 5", ActorType.CollidableProp, transform3DFallenTree5, effectParameters,
            this.modelDictionary["fallenTree"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            FallenTree5.Enable(true, 1);

            CollidableObject FallenTree6 = new TriangleMeshObject("fallen tree 6", ActorType.CollidableProp, transform3DFallenTree6, effectParameters,
            this.modelDictionary["fallenTree"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            FallenTree6.Enable(true, 1);


            this.object3DManager.Add(FallenTree1);
            this.object3DManager.Add(FallenTree2);
            this.object3DManager.Add(FallenTree3);
            this.object3DManager.Add(FallenTree4);
            this.object3DManager.Add(FallenTree5);
            this.object3DManager.Add(FallenTree6);
        }

        private void InitializeStaticCollidableElectricPole()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["electricPole"];
            effectParameters.DiffuseColor = Color.White;
            Vector3 poleRot = new Vector3(0, -90, 90);
            Vector3 poleScale = new Vector3(0.3f, 0.2f, 0.2f);
            Transform3D electricPole1 = new Transform3D(new Vector3(100, 5, -430), poleRot, poleScale, Vector3.UnitX, Vector3.UnitY);

            Transform3D electricPole2 = new Transform3D(new Vector3(60, 5, -90), poleRot, poleScale, Vector3.UnitX, Vector3.UnitY);

            //poleRot = new Vector3(90, 0, 0);
            Transform3D electricPole3 = new Transform3D(new Vector3(480, 5, -130), poleRot, poleScale, Vector3.UnitX, Vector3.UnitY);
            //poleRot =new Vector3(90,0,0);
            Transform3D electricPole4 = new Transform3D(new Vector3(480, 5, -430), poleRot, poleScale, Vector3.UnitX, Vector3.UnitY);


            CollidableObject ElectricPole1 = new TriangleMeshObject("fallen pole", ActorType.CollidableProp, electricPole1, effectParameters,
           this.modelDictionary["ElectricPole"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            ElectricPole1.Enable(true, 1);

            CollidableObject ElectricPole2 = new TriangleMeshObject("fallen pole 2", ActorType.CollidableProp, electricPole2, effectParameters,
            this.modelDictionary["ElectricPole"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            ElectricPole2.Enable(true, 1);


            CollidableObject ElectricPole3 = new TriangleMeshObject("fallen pole 3", ActorType.CollidableProp, electricPole3, effectParameters,
            this.modelDictionary["ElectricPole"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            ElectricPole3.Enable(true, 1);

            CollidableObject ElectricPole4 = new TriangleMeshObject("fallen pole 4", ActorType.CollidableProp, electricPole4, effectParameters,
            this.modelDictionary["ElectricPole"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            ElectricPole4.Enable(true, 1);

            this.object3DManager.Add(ElectricPole1);
            this.object3DManager.Add(ElectricPole2);
            this.object3DManager.Add(ElectricPole3);
            this.object3DManager.Add(ElectricPole4);

        }

        private void InitializeStaticCollidableFallenCar()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["ml"];
            effectParameters.DiffuseColor = Color.White;

            Transform3D car = new Transform3D(new Vector3(-930, 25, -240), new Vector3(0, 135, 0), new Vector3(0.2f, 0.2f, 0.2f), Vector3.UnitX, Vector3.UnitY);

            CollidableObject flippedCar = new TriangleMeshObject("flippedCar", ActorType.CollidableProp, car, effectParameters,
            this.modelDictionary["Car_for_game"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            flippedCar.Enable(true, 1);


            this.object3DManager.Add(flippedCar);
        }

        //Triangle mesh objects wrap a tight collision surface around complex shapes - the downside is that TriangleMeshObjects CANNOT be moved
        private void InitializeStaticCollidableTriangleMeshObjects()
        {
            InitializeStaticCollidableSnowDrift();
            InitializeStaticCollidableFallenTree();
            InitializeStaticCollidableElectricPole();
            InitializeStaticCollidableFallenCar();

        }

        private void InitializeTrees(int worldScale)
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["tree"];
            effectParameters.DiffuseColor = Color.White;
            Transform3D transform3D = new Transform3D(new Vector3(-85, 130, -565), new Vector3(0, 0, 0), new Vector3(0.8f, 0.8f, 0.8f), Vector3.UnitX, Vector3.UnitY);
            ModelObject tree = null;
            Model model = this.modelDictionary["fallenTree"];

            ModelObject archtype = new ModelObject("tree - ", ActorType.Decorator, transform3D, effectParameters, model);
            int treesCount = worldScale / 250;
            int count = 0;
            for (int i = 0; i < treesCount; i++)
            {
               
                    tree = (ModelObject)archtype.Clone();
                    tree.ID += count;
                    count++;
                    Console.WriteLine(tree.ID);
                    tree.Transform = new Transform3D(new Vector3(-200 + 200 * i, -10, -700 ),
                        new Vector3(0, 0, 0), 0.5f * new Vector3(1, 1.2f, 1), Vector3.UnitX, Vector3.UnitY);
                    tree.StatusType = StatusType.Drawn;
                    this.object3DManager.Add(tree);
                

            }

            for (int i = 0; i < treesCount; i++)
            {
                
                    tree = (ModelObject)archtype.Clone();
                    tree.ID += count;
                    count++;
                    Console.WriteLine(tree.ID);
                    tree.StatusType = StatusType.Drawn;
                    tree.Transform = new Transform3D(new Vector3(-200 + 200 * i, -10, 0 ),
                        new Vector3(0, 0, 0), 0.5f * new Vector3(1, 1.2f, 1), Vector3.UnitX, Vector3.UnitY);
                    this.object3DManager.Add(tree);
                

            }


        }

        private void InitializeBuildings()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-100, 10, -400),
                new Vector3(0, 90, 0), 0.99f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["house-low-texture"];

            CollidableObject collidableObject = new TriangleMeshObject("house1", ActorType.CollidableArchitecture, transform3D,
                                effectParameters, this.modelDictionary["house"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);
        }

        private void InitializeWallsFences()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-920, 0, -410),
                 new Vector3(0, 0, 0), new Vector3(1.9f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["wall"];

            //left far back
            CollidableObject collidableObject = new TriangleMeshObject("wall", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));


            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right far back
            transform3D = new Transform3D(new Vector3(-920, 0, -240),
               new Vector3(0, 0, 0), new Vector3(1.9f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //left front
            transform3D = new Transform3D(new Vector3(-220, 0, -580),
              new Vector3(0, 0, 0), new Vector3(1.4f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);
            //left front - 2
            transform3D = new Transform3D(new Vector3(280, 0, -580),
              new Vector3(0, 0, 0), new Vector3(1.2f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right front
            transform3D = new Transform3D(new Vector3(-220, 0, -70),
              new Vector3(0, 0, 0), new Vector3(1.4f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right front - 2
            transform3D = new Transform3D(new Vector3(280, 0, -70),
              new Vector3(0, 0, 0), new Vector3(1.2f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //left mid
            transform3D = new Transform3D(new Vector3(-250, 0, -410),
              new Vector3(0, 90, 0), new Vector3(0.5f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right mid
            transform3D = new Transform3D(new Vector3(-250, 0, -240),
              new Vector3(0, -90, 0), new Vector3(0.5f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //left end
            transform3D = new Transform3D(new Vector3(700, 0, -410),
              new Vector3(0, 90, 0), new Vector3(0.5f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right end
            transform3D = new Transform3D(new Vector3(700, 0, -240),
              new Vector3(0, -90, 0), new Vector3(0.5f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);


            //left box mid
            transform3D = new Transform3D(new Vector3(-40, 0, -410),
              new Vector3(0, 0, 0), new Vector3(0.7f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //right box mid
            transform3D = new Transform3D(new Vector3(200, 0, -240),
              new Vector3(0, 180, 0), new Vector3(0.7f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //near box mid
            transform3D = new Transform3D(new Vector3(-40, 0, -240),
              new Vector3(0, 90, 0), new Vector3(0.5f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //far box mid
            transform3D = new Transform3D(new Vector3(200, 0, -410),
              new Vector3(0, -90, 0), new Vector3(0.5f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,
                            effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            //Middle of the final section
            transform3D = new Transform3D(new Vector3(420, 0, -280),new Vector3(0, 0, 0),new Vector3(0.35f, 0.2f, 0.5f),Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D,effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            transform3D = new Transform3D(new Vector3(550, 0, -280), new Vector3(0, 90, 0), new Vector3(0.35f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D, effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            transform3D = new Transform3D(new Vector3(420, 0, -410), new Vector3(0, -90, 0), new Vector3(0.35f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D, effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);

            transform3D = new Transform3D(new Vector3(540, 0, -400), new Vector3(0, 180, 0), new Vector3(0.35f, 0.2f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new TriangleMeshObject("wall 2", ActorType.CollidableArchitecture, transform3D, effectParameters, this.modelDictionary["Wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.object3DManager.Add(collidableObject);
        }

        private void InitializeRoad()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-830, -6, -318),
                 new Vector3(0, 90, 0), 2.4f * Vector3.One, 2.4f * Vector3.UnitX, 2.4f * Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["roadtxt"];

            ModelObject roadObject = new ModelObject("roadpiece", ActorType.Decorator, transform3D, effectParameters, this.modelDictionary["road"]);

            this.object3DManager.Add(roadObject);

            #region Clones

            //Middle Clones 1, 2, 3, 4, 15, 16, 17
            //Left Clones 5, 7, 9, 11, 13, 18
            //Right Clones 6, 8, 10, 12, 14, 19
            //clone 1
            ModelObject clone = null;
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(-650, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 2
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(-470, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);


            //clone 3
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(-290, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 4
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(-150, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);


            //clone 5
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(-130, -6, -488);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 6
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(-130, -6, -150);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 7
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(50, -6, -488);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 8
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(50, -6, -150);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 9
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(230, -6, -488);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 10
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(230, -6, -150);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 11
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(410, -6, -488);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 12
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(410, -6, -150);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);


            //clone 13
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(590, -6, -488);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 14
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(590, -6, -150);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 15
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(340, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 16
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(520, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 17
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(700, -6, -318);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 2.4f);
            this.object3DManager.Add(clone);

            //clone 18
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(675, -6, -488);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 0.7f);
            this.object3DManager.Add(clone);

            //clone 19
            clone = (ModelObject)roadObject.Clone();
            clone.Transform.Translation = new Vector3(675, -6, -150);
            clone.Transform.Rotation = new Vector3(0, 90, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(2.4f, 2.4f, 0.7f);
            this.object3DManager.Add(clone);


            #endregion


        }
        #endregion

        private void InitializeUI()
        {
            Transform2D transform = null;
            UITextureObject texture = null;

            transform = new Transform2D(new Vector2(40, 210), 0, new Vector2(0.7f, 0.77f), Vector2.Zero, new Integer2(20, 281));

            texture = new UITextureObject("thermoBar",
                ActorType.UITexture, StatusType.Drawn | StatusType.Update,
                transform, Color.White,
                SpriteEffects.None, 0f, this.textureDictionary["ThermoBar"]);

            texture.AttachController(new ThermoController("tc", ControllerType.Timer, PlayStatusType.Play, this.eventDispatcher));


            this.hudManager.Add(texture);

            transform = new Transform2D(new Vector2(20, 190), 0, new Vector2(0.2f, 0.23f), Vector2.Zero, new Integer2(20, 281));

            texture = new UITextureObject("thermometer",
                ActorType.UITexture, StatusType.Drawn,
                transform, Color.White,
                SpriteEffects.None, 0.5f, this.textureDictionary["Thermometer"]);


            this.hudManager.Add(texture);

            transform = new Transform2D(new Vector2(20, 540), 0, new Vector2(0.3f, 0.23f), Vector2.Zero, new Integer2(20, 281));

            texture = new UITextureObject("charHUD",
                ActorType.UITexture, StatusType.Drawn,
                transform, Color.White,
                SpriteEffects.None, 0.4f, this.textureDictionary["charHUD"]);


            this.hudManager.Add(texture);


            transform = new Transform2D(new Vector2(180, 540), 0, new Vector2(0.42f, 0.48f), Vector2.Zero, new Integer2(20, 281));

            texture = new UITextureObject("charProfileFinal",
                ActorType.UITexture, StatusType.Drawn,
                transform, Color.White,
                SpriteEffects.None, 0.5f, this.textureDictionary["charProfileFinal"]);


            this.hudManager.Add(texture);

            transform = new Transform2D(new Vector2(20, 547), 0, new Vector2(0.068f, 0.038f), Vector2.Zero, new Integer2(20, 281));

            texture = new UITextureObject("shovel",
                ActorType.UITexture,
                StatusType.Drawn,
                transform,
                Color.White,
                SpriteEffects.None,
                0.5f,
                this.textureDictionary["shovel"]);


            this.hudManager.Add(texture);


            transform = new Transform2D(new Vector2(30, 634), 0, new Vector2(0.062f, 0.0385f), Vector2.Zero, new Integer2(20, 281));

            texture = new UITextureObject("coat",
                ActorType.UITexture, StatusType.Drawn,
                transform, Color.White,
                SpriteEffects.None, 0.5f, this.textureDictionary["coat"]);


            this.hudManager.Add(texture);

        }


        private void InitializeMenu()
        {
            Transform2D transform = null;
            Texture2D texture = null;
            Vector2 position = Vector2.Zero;
            string sceneID = "", buttonID = "";
            Vector2 midPoint = Vector2.Zero;
            UITextureObject uiTextureObject = null, textureClone = null;


            #region Main Menu
            sceneID = "main menu";

            //retrieve the audio menu background texture
            texture = this.textureDictionary["iceSheet"];
            //scale the texture to fit the entire screen
            Vector2 scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("menuTexture",
                ActorType.UITexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));



            midPoint = new Vector2(this.textureDictionary["menuButton"].Width / 2.0f,
               this.textureDictionary["menuButton"].Height / 2.0f);
            //add start button
            buttonID = "startbtn";
            texture = this.textureDictionary["button_start"];
            transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            transform.Translation += new Vector2(600f, 305f);
            uiTextureObject = new UITextureObject(buttonID, ActorType.UITexture, StatusType.Update | StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 0.1f, texture);


            this.menuManager.Add(sceneID, uiTextureObject);


            //clone button 1 - audio
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "audiobtn";
            textureClone.Texture = this.textureDictionary["button_sound"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            textureClone.Transform.Translation += new Vector2(600f, 405f);

            this.menuManager.Add(sceneID, textureClone);

            //clone button 2 - controls
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "controlsbtn";
            textureClone.Texture = this.textureDictionary["button_controls"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            textureClone.Transform.Translation += new Vector2(600f, 505f);

            this.menuManager.Add(sceneID, textureClone);

            //clone button 3 - exit
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "exitbtn";
            textureClone.Texture = this.textureDictionary["button_exit"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            //move down on Y-axis for next button
            textureClone.Transform.Translation += new Vector2(600f, 605f);

            this.menuManager.Add(sceneID, textureClone);


            //uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
            // transform, Color.LightPink, SpriteEffects.None, 0.1f, texture, buttonText,
            // this.fontDictionary["menu"],
            // Color.DarkGray, new Vector2(0, 2));
            //this.menuManager.Add(sceneID, uiButtonObject);
            #endregion

            #region Audio Menu
            sceneID = "audio menu";

            //retrieve the audio menu background texture
            texture = this.textureDictionary["iceSheet"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("audiomenuTexture",
                ActorType.UITexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));



            midPoint = new Vector2(this.textureDictionary["menuButton"].Width / 2.0f,
               this.textureDictionary["menuButton"].Height / 2.0f);
            //add start button
            buttonID = "volumeUpbtn";

            //first button volume up
            texture = this.textureDictionary["button_volume-up"];
            transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            transform.Translation += new Vector2(620f, 205f);
            uiTextureObject = new UITextureObject(buttonID, ActorType.UITexture, StatusType.Update | StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 0.1f, texture);



            this.menuManager.Add(sceneID, uiTextureObject);


            //clone button 1 - volume down
            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "volumeDownbtn";
            textureClone.Texture = this.textureDictionary["button_volume-down"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            textureClone.Transform.Translation += new Vector2(620f, 405f);

            this.menuManager.Add(sceneID, textureClone);

            //clone button 2 - volume mute
            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "volumeMutebtn";
            textureClone.Texture = this.textureDictionary["button_volume-mute"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            textureClone.Transform.Translation += new Vector2(620f, 505f);

            this.menuManager.Add(sceneID, textureClone);

            //clone button 3 - volume unmute
            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "volumeUnMutebtn";
            textureClone.Texture = this.textureDictionary["button_unmute"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            //move down on Y-axis for next button
            textureClone.Transform.Translation += new Vector2(620f, 605f);
            //change the texture blend color
            //textureClone.Color = Color.White;

            this.menuManager.Add(sceneID, textureClone);

            //clone button 4 - Exit
            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "backbtn";
            textureClone.Texture = this.textureDictionary["button_back"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            //move down on Y-axis for next button
            textureClone.Transform.Translation += new Vector2(620f, 705f);


            this.menuManager.Add(sceneID, textureClone);



            transform = new Transform2D(new Vector2(200, 230), 0, new Vector2(1, 0.5f), Vector2.Zero, new Integer2(600, 160));

            uiTextureObject = new UITextureObject("slider1", ActorType.UITexture,
               StatusType.Drawn | StatusType.Update, transform,
               Color.White, SpriteEffects.None, 0.1f,
               this.textureDictionary["sliderBar"]);

            this.menuManager.Add(sceneID, uiTextureObject);

            transform = new Transform2D(new Vector2(300, 220), 0, new Vector2(1, 0.7f), Vector2.Zero, new Integer2(180, 150));

            uiTextureObject = new UITextureObject("tracker1", ActorType.UITexture,
                StatusType.Drawn | StatusType.Update, transform,
                Color.White, SpriteEffects.None, 0,
               this.textureDictionary["sliderTracker"]);

            this.menuManager.Add(sceneID, uiTextureObject);


            #endregion

            #region Controls Menu
            sceneID = "controls menu";

           
            texture = this.textureDictionary["controlBackground"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("controlsmenuTexture", ActorType.UITexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "backbtn";
            textureClone.Texture = this.textureDictionary["button_back"];
            textureClone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(200, 50));
            //move down on Y-axis for next button
            textureClone.Transform.Translation += new Vector2(620f, 755f);


            this.menuManager.Add(sceneID, textureClone);
            #endregion

            #region PauseMenu

            sceneID = "pause menu";

            #region backgroundTexture
            texture = this.textureDictionary["iceSheet"];
            //scale the texture to fit the entire screen

            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("audiomenuTexture",
                ActorType.UITexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            #endregion

            #region Buttons



            //retrieve the audio menu background texture
            texture = this.textureDictionary["iceSheet"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("mainmenuTexture",
                ActorType.UITexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));



            midPoint = new Vector2(this.textureDictionary["menuButton"].Width / 2.0f,
               this.textureDictionary["menuButton"].Height / 2.0f);
            //add start button
            buttonID = "audiobtn";
            //first button sound
            texture = this.textureDictionary["button_sound"];
            transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200),
              0, 1.5f * Vector2.One, midPoint, new Integer2(1000, 367));
            transform.Translation += new Vector2(620f, 205f);
            uiTextureObject = new UITextureObject(buttonID, ActorType.UITexture, StatusType.Update | StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 0.1f, texture);



            this.menuManager.Add(sceneID, uiTextureObject);


            //clone 1 controls button
            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "controlsbtn";
            textureClone.Texture = this.textureDictionary["button_controls"];

            textureClone.Transform.Translation += new Vector2(0f, 150f);
            //change the texture blend color
            textureClone.Color = Color.White;

            this.menuManager.Add(sceneID, textureClone);

            //clone 2 exit button
            //add controls button - clone the audio button then just reset texture, ids etc in all the clones
            textureClone = (UITextureObject)uiTextureObject.Clone();
            textureClone.ID = "exitbtn";

            //move down on Y-axis for next button
            textureClone.Transform.Translation += new Vector2(0f, 300f);
            //change the texture blend color
            textureClone.Color = Color.White;
            textureClone.Texture = this.textureDictionary["button_exit"];
            this.menuManager.Add(sceneID, textureClone);

            #endregion


            #endregion

        }

        private void InitializeDictionaries()
        {
            //textures, models, fonts
            this.modelDictionary = new ContentDictionary<Model>("model dictionary", this.Content);
            this.textureDictionary = new ContentDictionary<Texture2D>("texture dictionary", this.Content);
            this.fontDictionary = new ContentDictionary<SpriteFont>("font dictionary", this.Content);

            //rail, transform3Dcurve               
            this.railDictionary = new Dictionary<string, RailParameters>();
            this.track3DDictionary = new Dictionary<string, Track3D>();

            //effect parameters
            this.effectDictionary = new Dictionary<string, EffectParameters>();
        }

#if DEBUG
        private void InitializeDebug(bool v)
        {
           DebugDrawer debugDrawer = new DebugDrawer(this, this.cameraManager,
                this.eventDispatcher,
                StatusType.Off,
                this.spriteBatch, this.fontDictionary["debugFont"], new Vector2(20, 20), Color.White);

            debugDrawer.DrawOrder = 4;
            Components.Add(debugDrawer);
        }

        private void InitializeDebugCollisionSkinInfo()
        {
            //show the collision skins
            this.physicsDebugDrawer = new PhysicsDebugDrawer(this, this.cameraManager, this.object3DManager,
                this.eventDispatcher, StatusType.Off, this.cameraLayoutType);
            Components.Add(this.physicsDebugDrawer);
        }
#endif

        #region Assets
        private void LoadAssets()
        {
            LoadTextures();
            LoadModels();
            LoadFonts();
            LoadRails();
            LoadTracks();
        }

        private void LoadFonts()
        {
            this.fontDictionary.Load("hudFont", "Assets/Fonts/hudFont");
            this.fontDictionary.Load("menu", "Assets/Fonts/menu");
            this.fontDictionary.Load("debugFont", "Assets/Debug/Fonts/debugFont");
        }

        private void LoadModels()
        {
            #region Models
            //geometric samples
            this.modelDictionary.Load("Assets/Models/plane1");
            this.modelDictionary.Load("Assets/Models/plane");
            this.modelDictionary.Load("Assets/Models/box2");
            this.modelDictionary.Load("Assets/Models/torus");
            this.modelDictionary.Load("Assets/Models/fallenTree");
            this.modelDictionary.Load("Assets/Models/Car_for_game");
            this.modelDictionary.Load("Assets/Models/ElectricPole");
            this.modelDictionary.Load("Assets/Models/snow_drift");
            this.modelDictionary.Load("Assets/Models/road");
            this.modelDictionary.Load("Assets/Models/sphere");
            this.modelDictionary.Load("mapLayout", "Assets/Models/mapBlockingOut");
            this.modelDictionary.Load("Assets/Models/Character_model_1");
            this.modelDictionary.Load("Assets/Models/icycle");
            this.modelDictionary.Load("Assets/Models/tree_centred");

            //triangle mesh high/low poly demo
            this.modelDictionary.Load("Assets/Models/teapot");
            this.modelDictionary.Load("Assets/Models/teapot_mediumpoly");
            this.modelDictionary.Load("Assets/Models/teapot_lowpoly");

            //player - replace with animation eventually
            this.modelDictionary.Load("Assets/Models/cylinder");

            //architecture
            this.modelDictionary.Load("Assets/Models/Architecture/Buildings/house");
            this.modelDictionary.Load("Assets/Models/Wall");
            this.modelDictionary.Load("Assets/Models/RoadRoof");

            //dual texture demo
            this.modelDictionary.Load("Assets/Models/box");
            this.modelDictionary.Load("Assets/Models/box1");
            #endregion

            #region Animations
            //contains a single animation "Take001"
            this.modelDictionary.Load("Assets/Models/Animated/dude");

            //squirrel - one file per animation
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Idle");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Jump");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Punch");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Standing");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Tailwhip");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/RedRun4");
            this.modelDictionary.Load("Assets/Models/Animated/Player/char_walk");
            this.modelDictionary.Load("Assets/Models/Animated/Player/char_idle");
            this.modelDictionary.Load("Assets/Models/Animated/Player/char_fall");
            #endregion

        }

        private void LoadTextures()
        {
            #region Textures
            //environment
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate1"); //demo use of the shorter form of Load() that generates key from asset name
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate2");
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");
            this.textureDictionary.Load("Assets/Textures/Skybox/back");
            this.textureDictionary.Load("Assets/Textures/Skybox/left");
            this.textureDictionary.Load("Assets/Textures/Skybox/right");
            this.textureDictionary.Load("Assets/Textures/Skybox/sky");
            this.textureDictionary.Load("Assets/Textures/Skybox/front");
            this.textureDictionary.Load("Assets/Textures/GroundTexture/iceSheet");
            this.textureDictionary.Load("Assets/Textures/GroundTexture/snow");
            this.textureDictionary.Load("Assets/Textures/Foliage/Trees/tree");
            this.textureDictionary.Load("Assets/Textures/Foliage/Pole/electricPole");
            this.textureDictionary.Load("Assets/Textures/Foliage/Car/carTexture");

            //dual texture demo
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass_midlevel");
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass_highlevel");

            //menu - buttons
            this.textureDictionary.Load("Assets/Textures/Menu/Button");
            this.textureDictionary.Load("Assets/Textures/Menu/startButton");
            this.textureDictionary.Load("Assets/Textures/Menu/Menu");

            //menu - backgrounds
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/mainmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/audiomenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/controlBackground");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/grey");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/keyboardControl");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/exitmenuwithtrans");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_back");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_controls");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_exit");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_sound");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_start");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_volume-down");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_volume-mute");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_unmute");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/button_volume-up");


            //ui (or hud) elements
            this.textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/charHUD");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/charProfileFinal");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/coat");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/shovel");

            this.textureDictionary.Load("Assets/Textures/Char/charTexture");

            //architecture
            this.textureDictionary.Load("Assets/Textures/Architecture/Buildings/house-low-texture");
            this.textureDictionary.Load("Assets/Textures/Architecture/Walls/wall");

            //dual texture demo - see Main::InitializeCollidableGround()
            this.textureDictionary.Load("Assets/Debug/Textures/checkerboard_greywhite");

            //debug
            this.textureDictionary.Load("Assets/Debug/Textures/checkerboard");
            this.textureDictionary.Load("Assets/Debug/Textures/ml");
            this.textureDictionary.Load("Assets/Debug/Textures/checkerboard");


            this.textureDictionary.Load("Assets/Textures/Menu/exitButton");
            this.textureDictionary.Load("Assets/Textures/Menu/menuButton");
            this.textureDictionary.Load("Assets/Textures/Menu/sliderBar");
            this.textureDictionary.Load("Assets/Textures/Menu/sliderTracker");

            this.textureDictionary.Load("Assets/Textures/UI/HUD/ThermoBar");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/Thermometer");
            this.textureDictionary.Load("Assets/Textures/Road/roadtxt");
            this.textureDictionary.Load("Assets/Textures/UI/FreezeOver/freezeOver_001");
            this.textureDictionary.Load("Assets/Textures/UI/FreezeOver/freezeOver_002");
            this.textureDictionary.Load("Assets/Textures/UI/FreezeOver/freezeOver_003");
            this.textureDictionary.Load("Assets/Textures/UI/FreezeOver/freezeOver_004");
            this.textureDictionary.Load("Assets/Textures/UI/FreezeOver/cheese");


            #endregion
        }

        private void InitializeLevelOutline()
        {
            #region fields
            Transform3D transform = null;

            Vector3 groundPanelTranslation = new Vector3(1, 2, -300);
            Vector3 groundPanelRotation = new Vector3(-90, 0, 0);
            Vector3 groundPanelScale = new Vector3(0.5f, 0.5f, 0.5f);

            #endregion
            #region pavement
            transform = new Transform3D(groundPanelTranslation, groundPanelScale);

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["iceSheet"];

            //create a archetype to use for cloning
            ModelObject mapLayout = new ModelObject("mapLayout", ActorType.Decorator, transform, effectParameters, this.modelDictionary["mapLayout"]);
            this.object3DManager.Add(mapLayout);
            #endregion  

        }

        private void LoadRails()
        {
            RailParameters railParameters = null;

            //create a simple rail that gains height as the target moves on +ve X-axis - try different rail vectors
            railParameters = new RailParameters("battlefield 1", new Vector3(0, 10, 50), new Vector3(50, 50, 50));
            this.railDictionary.Add(railParameters.ID, railParameters);

            //add more rails here...
            railParameters = new RailParameters("battlefield 2", new Vector3(-50, 20, 20), new Vector3(50, 80, 100));
            this.railDictionary.Add(railParameters.ID, railParameters);
        }

        private void LoadTracks()
        {
            Track3D track3D = null;

            //starts away from origin, moves forward and rises, then ends closer to origin and looking down from a height
            track3D = new Track3D(CurveLoopType.Oscillate);
            track3D.Add(new Vector3(0, 10, 200), -Vector3.UnitZ, Vector3.UnitY, 0);
            track3D.Add(new Vector3(0, 20, 150), -Vector3.UnitZ, Vector3.UnitY, 2);
            track3D.Add(new Vector3(0, 40, 100), -Vector3.UnitZ, Vector3.UnitY, 4);

            //set so that the camera looks down at the origin at the end of the curve
            Vector3 finalPosition = new Vector3(0, 80, 50);
            Vector3 finalLook = Vector3.Normalize(Vector3.Zero - finalPosition);

            track3D.Add(finalPosition, finalLook, Vector3.UnitY, 6);
            this.track3DDictionary.Add("push forward 1", track3D);

            //add more transform3D curves here...
        }

        #endregion

        #region Graphics & Effects
        private void InitializeEffects()
        {
            BasicEffect basicEffect = null;
            DualTextureEffect dualTextureEffect = null;

            #region Lit objects
            //create a BasicEffect and set the lighting conditions for all models that use this effect in their EffectParameters field
            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.TextureEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            this.effectDictionary.Add(AppData.LitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For Unlit objects
            //used for model objects that dont interact with lighting i.e. sky
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.LightingEnabled = false;
            this.effectDictionary.Add(AppData.UnlitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For dual texture objects
            dualTextureEffect = new DualTextureEffect(graphics.GraphicsDevice);
            this.effectDictionary.Add(AppData.UnlitModelDualEffectID, new DualTextureEffectParameters(dualTextureEffect));
            #endregion
        }

        private void InitializeGraphics()
        {
            this.graphics.PreferredBackBufferWidth = resolution.X;
            this.graphics.PreferredBackBufferHeight = resolution.Y;

            //solves the skybox border problem
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Clamp;
            samplerState.AddressV = TextureAddressMode.Clamp;
            this.graphics.GraphicsDevice.SamplerStates[0] = samplerState;

            //enable alpha transparency - see ColorParameters
            this.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            this.graphics.ApplyChanges();
        }
        #endregion

        #region Cameras
        private void InitializeCameras(ScreenLayoutType screenLayoutType)
        {
            Viewport viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            float aspectRatio = (float)this.resolution.X / this.resolution.Y;
            ProjectionParameters projectionParameters
                = new ProjectionParameters(MathHelper.PiOver4, aspectRatio, 1, 4000);

            if (screenLayoutType == ScreenLayoutType.FirstPerson)
            {
                AddFirstPersonCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.ThirdPerson)
            {
                AddThirdPersonCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.Flight)
            {
                AddFlightCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.Rail)
            {
                //   AddRailCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.Track)
            {
                AddTrack3DCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.Pip)
            {
                AddMainAndPipCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.Multi1x4) //splits the screen vertically x4
            {
                viewport = new Viewport(0, 0, (int)(graphics.PreferredBackBufferWidth / 4.0f), graphics.PreferredBackBufferHeight);
                AddFirstPersonCamera(viewport, projectionParameters);

                //   viewport.X += viewport.Width; //move the next camera over to start at x = 1/4 screen width
                //   AddRailCamera(viewport, projectionParameters);

                viewport.X += viewport.Width; //move the next camera over to start at x = 2/4 screen width
                AddTrack3DCamera(viewport, projectionParameters);

                viewport.X += viewport.Width; //move the next camera over to start at x = 3/4 screen width
                AddSecurityCamera(viewport, projectionParameters);
            }
            else if (screenLayoutType == ScreenLayoutType.Multi2x2) //splits the screen in 4 equal parts
            {
                //top left
                viewport = new Viewport(0, 0, (int)(graphics.PreferredBackBufferWidth / 2.0f), (int)(graphics.PreferredBackBufferHeight / 2.0f));
                AddFirstPersonCamera(viewport, projectionParameters);

                //top right
                //   viewport.X = viewport.Width; 
                //   AddRailCamera(viewport, projectionParameters);

                ////bottom left
                viewport.X = 0;
                viewport.Y = viewport.Height;
                AddTrack3DCamera(viewport, projectionParameters);

                ////bottom right
                viewport.X = viewport.Width;
                viewport.Y = viewport.Height;
                AddSecurityCamera(viewport, projectionParameters);
            }
            else //in all other cases just add a security camera - saves us having to implement all enum options at the moment
            {
                AddSecurityCamera(viewport, projectionParameters);
            }
        }

        private void AddMainAndPipCamera(Viewport viewport, ProjectionParameters projectionParameters)
        {
            Camera3D camera3D = null;
            Transform3D transform = null;

            //security camera
            transform = new Transform3D(new Vector3(0, 40, 0),
                Vector3.Zero, Vector3.One, -Vector3.UnitY, Vector3.UnitZ);

            int width = 240;
            int height = 180;
            int xPos = this.resolution.X - width - 10;
            Viewport pipViewport = new Viewport(xPos, 10, width, height);

            camera3D = new Camera3D("sc1", ActorType.Camera, transform,
                projectionParameters, pipViewport,
                0f, StatusType.Update);


            camera3D.AttachController(new SecurityCameraController("scc1", ControllerType.Security, 15, 2, Vector3.UnitX));

            this.cameraManager.Add(camera3D);

            //1st person
            transform = new Transform3D(
                 new Vector3(0, 10, 100), Vector3.Zero,
                 Vector3.One, -Vector3.UnitZ, Vector3.UnitY);

            camera3D = new Camera3D("fpc1", ActorType.Camera, transform,
                projectionParameters, viewport,
                1f, StatusType.Update);

            camera3D.AttachController(new FirstPersonCameraController(
              "fpcc1", ControllerType.FirstPerson,
              AppData.CameraMoveKeys, AppData.CameraMoveSpeed,
              AppData.CameraStrafeSpeed, AppData.CameraRotationSpeed, this.inputManagerParameters, this.screenCentre));

            //put controller later!
            this.cameraManager.Add(camera3D);
        }

        private void AddTrack3DCamera(Viewport viewport, ProjectionParameters projectionParameters)
        {
            //doesnt matter where the camera starts because we reset immediately inside the Transform3DCurveController
            Transform3D transform = Transform3D.Zero;

            Camera3D camera3D = new Camera3D("curve camera 1",
                ActorType.Camera, transform,
                projectionParameters, viewport,
                0f, StatusType.Update);

            camera3D.AttachController(new Track3DController("tcc1", ControllerType.Track,
                this.track3DDictionary["push forward 1"], PlayStatusType.Play));

            this.cameraManager.Add(camera3D);
        }

        //private void AddRailCamera(Viewport viewport, ProjectionParameters projectionParameters)
        //{
        //    //doesnt matter where the camera starts because we reset immediately inside the RailController
        //    Transform3D transform = Transform3D.Zero;

        //    Camera3D camera3D = new Camera3D("rail camera 1",
        //        ActorType.Camera, transform,
        //        ProjectionParameters.StandardMediumFiveThree, viewport,
        //        0f, StatusType.Update);


        //    camera3D.AttachController(new RailController("rc1", ControllerType.Rail, 
        //        this.drivableModelObject, this.railDictionary["battlefield 1"]));

        //    this.cameraManager.Add(camera3D);

        //}

        private void AddThirdPersonCamera(Viewport viewport, ProjectionParameters projectionParameters)
        {
            Transform3D transform =
                new Transform3D(
                    new Vector3(0, 0, -50),
                    Vector3.Zero,
                    Vector3.One,
                    Vector3.UnitZ,
                    Vector3.UnitY);

            Camera3D camera3D = new Camera3D("third person camera 1",
                ActorType.Camera, transform,
                ProjectionParameters.StandardDeepFiveThree, viewport,
                0f, StatusType.Update);

            camera3D.AttachController(new ThirdPersonController("tpcc1", ControllerType.ThirdPerson,
                this.animatedHeroPlayerObject, AppData.CameraThirdPersonDistance,
                AppData.CameraThirdPersonScrollSpeedDistanceMultiplier,
                AppData.CameraThirdPersonElevationAngleInDegrees,
                AppData.CameraThirdPersonScrollSpeedElevationMultiplier,
                LerpSpeed.Slow, LerpSpeed.VerySlow, this.inputManagerParameters));

            this.cameraManager.Add(camera3D);

        }

        private void AddSecurityCamera(Viewport viewport, ProjectionParameters projectionParameters)
        {
            Transform3D transform = new Transform3D(new Vector3(50, 10, 10), Vector3.Zero, Vector3.Zero, -Vector3.UnitX, Vector3.UnitY);

            Camera3D camera3D = new Camera3D("security camera 1",
                ActorType.Camera, transform,
                projectionParameters, viewport,
                0f, StatusType.Update);

            camera3D.AttachController(new SecurityCameraController("scc1", ControllerType.Security, 15, 2, Vector3.UnitX));

            this.cameraManager.Add(camera3D);

        }

        private void AddFlightCamera(Viewport viewport, ProjectionParameters projectionParameters)
        {
            Transform3D transform = new Transform3D(new Vector3(0, 10, 30), Vector3.Zero, Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            Camera3D camera3D = new Camera3D("flight camera 1",
                ActorType.Camera, transform,
                projectionParameters, viewport,
                0f, StatusType.Update);

            camera3D.AttachController(new FlightCameraController("flight camera controller 1",
                ControllerType.Flight, AppData.CameraMoveKeys_Alt1, AppData.CameraMoveSpeed,
                AppData.CameraStrafeSpeed, AppData.CameraRotationSpeed, this.inputManagerParameters, this.screenCentre));

            this.cameraManager.Add(camera3D);
        }

        private void AddFirstPersonCamera(Viewport viewport, ProjectionParameters projectionParameters)
        {
            Transform3D transform = new Transform3D(new Vector3(0, 10, 80), Vector3.Zero, Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            Camera3D camera3D = new Camera3D("first person camera 1",
                ActorType.Camera, transform,
                projectionParameters, viewport,
                0f, StatusType.Update);

            camera3D.AttachController(new FirstPersonCameraController(
                "fpcc1", ControllerType.FirstPerson,
                AppData.CameraMoveKeys, AppData.CameraMoveSpeed,
                AppData.CameraStrafeSpeed, AppData.CameraRotationSpeed, this.inputManagerParameters, this.screenCentre));

            this.cameraManager.Add(camera3D);

        }
        #endregion

        #region Load/Unload, Draw, Update
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //// Create a new SpriteBatch, which can be used to draw textures.
            //spriteBatch = new SpriteBatch(GraphicsDevice);

            ////since debug needs sprite batch then call here
            //InitializeDebug(true);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            this.modelDictionary.Dispose();
            this.fontDictionary.Dispose();
            this.textureDictionary.Dispose();

            //only C# dictionary so no Dispose() method to call
            this.railDictionary.Clear();
            this.track3DDictionary.Clear();

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            DemoUseItem();

            ZoneEvents();

            DemoFreezeOver();

            DemoSetControllerPlayStatus();

            DemoSoundManager();

            DemoToggleMenu();

            ToggleDebugInfo();


            DemoGameOver();

            DemoUseText();

            //DemoEquipItem();

            base.Update(gameTime);
        }

        private void DemoFreezeOver()
        {
            this.eventDispatcher.LowTemp += EventDispacher_TempDrop;
        }

        private bool temp1 = true;
        private bool temp2 = true;
        private bool temp3 = true;
        private bool temp4 = true;
        private HeroAnimatedPlayerObject animatedHeroPlayerObject;

        private void EventDispacher_TempDrop(EventData eventData)
        {  //string id = eventData
            Transform2D transform = new Transform2D(Vector2.One);
            Texture2D texture = textureDictionary["cheese"];

            UITextureObject newTexture = new UITextureObject("freezeOverTexture1",
                ActorType.UITexture,
                StatusType.Drawn,
                transform,
                Color.White,
                SpriteEffects.None,
                1.0f,
                texture);

            if (eventData.ID == "tempBelow20" && temp1)
            {
                temp1 = false;
                texture = textureDictionary["freezeOver_001"];
                newTexture = new UITextureObject("freezeOverTexture1",
                ActorType.UITexture,
                StatusType.Drawn,
                transform,
                Color.White,
                SpriteEffects.None,
                1.0f,
                texture);
                
                EventDispatcher.Publish(new EventData(newTexture
                        , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));
            }
            else if (eventData.ID == "tempBelow10" && temp2)
            {
                temp2 = false;
                EventDispatcher.Publish(new EventData(newTexture
                        , EventActionType.OnRemoveActor2D, EventCategoryType.SystemRemove));
                texture = textureDictionary["freezeOver_002"];
                newTexture = new UITextureObject("freezeOverTexture2",
                ActorType.UITexture,
                StatusType.Drawn,
                transform,
                Color.White,
                SpriteEffects.None,
                0.99f,
                texture);

                EventDispatcher.Publish(new EventData(newTexture
                        , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));


            }
            else if (eventData.ID == "tempBelow5" && temp3)
            {
                temp3 = false;
                texture = textureDictionary["freezeOver_003"];
                newTexture = new UITextureObject("freezeOverTexture3",
                ActorType.UITexture,
                StatusType.Drawn,
                transform,
                Color.White,
                SpriteEffects.None,
                0.98f,
                texture);

                EventDispatcher.Publish(new EventData(newTexture
                        , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));

            }
            else if (eventData.ID == "tempBelow1" && temp4)
            {
                temp4 = false;
                texture = textureDictionary["freezeOver_004"];
                newTexture = new UITextureObject("freezeOverTexture4",
                ActorType.UITexture,
                StatusType.Drawn,
                transform,
                Color.White,
                SpriteEffects.None,
                0.97f,
                texture);

                EventDispatcher.Publish(new EventData(newTexture
                        , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));

            }
        }


        private void ZoneEvents()
        {
            this.eventDispatcher.ObstacleEvent += EventDispatcher_ObstacleEvent;
        }

        private void EventDispatcher_ObstacleEvent(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnIcicleZone)
            {
                string id = "clone - icicle - " + eventData.AdditionalParameters[0];
                Actor3D icicle = this.object3DManager.Find(i => i.ActorType.Equals(ActorType.Icicle) && i.ID.Equals(id));
                icicle.SetAllControllersPlayStatus(PlayStatusType.Play);
            }
            else if (eventData.EventType == EventActionType.OnTreeZone)
            {
                Actor3D fallingTree = this.object3DManager.Find(i => i.ActorType.Equals(ActorType.FallingTree));
                fallingTree.SetAllControllersPlayStatus(PlayStatusType.Play);
            }
        }

        private void DemoUseText()
        {
            this.eventDispatcher.ObstacleCollision += EventDispatcher_ObstacleCollision;
        }

        private void EventDispatcher_ObstacleCollision(EventData eventData)
        {
            string text = "use";
            Vector2 pos = new Vector2(70, 600);
            SpriteFont strFont = this.fontDictionary["menu"];
            Vector2 strDim = strFont.MeasureString(text);
            strDim /= 2.0f;
            Transform2D transform =
                new Transform2D(pos, 0,
                new Vector2(1, 1), strDim, new Integer2(100, 100));

            UITextObject newTextObject = new UITextObject("use indicator", ActorType.UIText,
                StatusType.Drawn | StatusType.Update, transform, Color.Red,
                SpriteEffects.None, 0, text, strFont);
            if (!this.once && !this.added)
            {
                this.once = true;
            }

            if (eventData.EventType == EventActionType.OnSnowDrift)
            {

                //newTextObject.Text = "Use";
                if (this.once)
                {

                    Console.WriteLine("add");
                    EventDispatcher.Publish(new EventData(newTextObject
                     , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));
                    this.once = false;
                    this.added = true;
                }
            }
            else if (eventData.EventType == EventActionType.OnGround)
            {
                if (this.added)
                {
                    Console.WriteLine("Remove");
                    EventDispatcher.Publish(new EventData(newTextObject
                     , EventActionType.OnRemoveActor2D, EventCategoryType.SystemRemove));
                    //this.once = true;
                    this.added = false;
                }

            }



        }



        private void DemoGameOver()
        {
            this.eventDispatcher.GameLost += EventDispatcher_GameLost;
            this.eventDispatcher.GameWon += EventDispatcher_GameWon;
        }

        private void EventDispatcher_GameWon(EventData eventData)
        {
            if(eventData.EventType == EventActionType.OnGameWin)
            {
                if(!this.gameOver)
                {
                    this.gameOver = true;
                    string text = eventData.ID;
                    SpriteFont strFont = this.fontDictionary["menu"];
                    Vector2 strDim = strFont.MeasureString(text);
                    strDim /= 2.0f;

                    Transform2D transform =
                        new Transform2D((Vector2)this.screenCentre, 0,
                        new Vector2(1, 1) * 2, strDim, new Integer2(100, 100));

                    UITextObject newTextObject = new UITextObject("lose", ActorType.UIText,
                        StatusType.Drawn | StatusType.Update, transform, Color.Red,
                        SpriteEffects.None, 0, text, strFont);


                    EventDispatcher.Publish(new EventData(newTextObject
                            , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));
                }
                
            }
        }

        private void EventDispatcher_GameLost(EventData eventData)
        {
            if (!this.gameOver)
            {
                this.gameOver = true;
                string text = eventData.ID;
                SpriteFont strFont = this.fontDictionary["menu"];
                Vector2 strDim = strFont.MeasureString(text);
                strDim /= 2.0f;

                Transform2D transform =
                    new Transform2D((Vector2)this.screenCentre, 0,
                    new Vector2(1, 1) * 2, strDim, new Integer2(100, 100));

                UITextObject newTextObject = new UITextObject("lose", ActorType.UIText,
                    StatusType.Drawn | StatusType.Update, transform, Color.Red,
                    SpriteEffects.None, 0, text, strFont);


                EventDispatcher.Publish(new EventData(newTextObject
                        , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));
            }

        }

        private void ToggleDebugInfo()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.T))
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnToggle, EventCategoryType.Debug));
            }
        }


        private void DemoUseItem()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.Enter))
            {// wear coat using enter
                DemoItem("shovel");
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.C))
            {

                DemoItem("coat");
                DemoEquipItem();
            }
        }

        private void DemoEquipItem()
        {
            string text = "Equipped";
            SpriteFont strFont = this.fontDictionary["menu"];
            Vector2 strDim = strFont.MeasureString(text);
            Vector2 pos = Vector2.Zero;
            strDim /= 2.0f;

            pos = new Vector2(100, 700);
            Transform2D transform =
                new Transform2D(pos, 0,
                new Vector2(1, 1), strDim, new Integer2(100, 100));

            UITextObject newTextObject = new UITextObject("equip", ActorType.UIText,
                StatusType.Drawn | StatusType.Update, transform, Color.Red,
                SpriteEffects.None, 0, text, strFont);

            if (!this.coat)
            {
                EventDispatcher.Publish(new EventData(newTextObject
                    , EventActionType.OnAddActor2D, EventCategoryType.SystemAdd));
                this.coat = true;
            }
            else
            {
                EventDispatcher.Publish(new EventData(newTextObject
                   , EventActionType.OnRemoveActor2D, EventCategoryType.SystemRemove));
                this.coat = false;
            }





        }

        private void DemoItem(string itemID)
        {
            //publish event and sending what the id of the item
            object[] additionalParameters = { itemID };
            if (itemID.Equals("coat"))
                EventDispatcher.Publish(new EventData(EventActionType.OnItem, EventCategoryType.Item, additionalParameters));
            if (itemID.Equals("shovel"))

                EventDispatcher.Publish(new EventData(EventActionType.OnItem, EventCategoryType.ItemEquipped, additionalParameters));

        }


        private void DemoToggleMenu()
        {
            if (this.keyboardManager.IsFirstKeyPress(AppData.MenuShowHideKey))
            {
                this.menuManager.SetActiveList("pause menu");
                if (this.menuManager.IsVisible)
                    EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.Menu));
                else
                    EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.Menu));
            }
        }

        private void DemoSoundManager()
        {


            if (this.keyboardManager.IsFirstKeyPress(Keys.B))
            {
                //add event to play mouse click
                object[] additionalParameters = { "boing" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.NumPad1))
            {
                //add event to play mouse click
                object[] additionalParameters = { "Oof" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.NumPad2))
            {
                //add event to play mouse click
                object[] additionalParameters = { "Ouch" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.NumPad3))
            {
                //add event to play mouse click
                object[] additionalParameters = { "Gasp" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.NumPad4))
            {
                //add event to play mouse click
                object[] additionalParameters = { "PhoneCall" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }
        }

        private void DemoSetControllerPlayStatus()
        {
            Actor3D torusActor = this.object3DManager.Find(actor => actor.ID.Equals("torus 1"));
            if (torusActor != null && this.keyboardManager.IsFirstKeyPress(Keys.O))
            {
                torusActor.SetControllerPlayStatus(PlayStatusType.Pause, controller => controller.GetControllerType() == ControllerType.Rotation);
            }
            else if (torusActor != null && this.keyboardManager.IsFirstKeyPress(Keys.P))
            {
                torusActor.SetControllerPlayStatus(PlayStatusType.Play, controller => controller.GetControllerType() == ControllerType.Rotation);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }
        #endregion
    }
}

