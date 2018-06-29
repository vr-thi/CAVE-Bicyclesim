# CAVE-Bicyclesim
Bicycle simulator for CAVE


## Assets: 

### Buildings: 
The buildings in the scene come from the cadnav marketplace. 
All 3DModels are free and were converted with the tool 3DS Max into Unity compatible formats. 

The following list contains a few extra buildings which are not used in the scene, but fit the artstyle: 

- Old Russian commercial district building 3D Model
http://www.cadnav.com/3d-models/model-34138.html
- Terraced house 3D Model
http://www.cadnav.com/3d-models/model-30966.html
- Vintage terraced house 3D Model
http://www.cadnav.com/3d-models/model-30981.html
- City residence townhouse 3D Model
http://www.cadnav.com/3d-models/model-11061.html
- Semi-detached townhouse 3D Model
http://www.cadnav.com/3d-models/model-11060.html
- Semi-detached townhouse 3D Model
http://www.cadnav.com/3d-models/model-11054.html
- Elderly apartment housing 3D Model
http://www.cadnav.com/3d-models/model-11039.html

- Ostozhenka Moscow Building 3D Model
http://www.cadnav.com/3d-models/model-38234.html
- Brick apartment building 3D Model
http://www.cadnav.com/3d-models/model-34148.html
- Traditional Russian mansion 3D Model
http://www.cadnav.com/3d-models/model-34135.html
- Old apartment building 3D Model
http://www.cadnav.com/3d-models/model-31019.html
- Italianate dwelling house 3D Model
http://www.cadnav.com/3d-models/model-30961.html
- German suburb of apartment building 3D Model
http://www.cadnav.com/3d-models/model-30945.html
- Apartment block 3D Model
http://www.cadnav.com/3d-models/model-8525.


### Unity Asset Store 

- Trees: 
https://assetstore.unity.com/packages/3d/vegetation/trees/realistic-tree-9-rainbow-tree-54622

- Street Probs 
https://assetstore.unity.com/packages/3d/environments/urban/low-poly-street-pack-67475

- Materials: 
https://assetstore.unity.com/packages/2d/textures-materials/roads/yughues-free-pavement-materials-12952



## Working with the Cave - common guidelines and rules

- First of all read the CaveWiki and it's quick start guide. This guide provides additional informations, which are sometimes generic cave issues and sometimes project specific. 


#### Naming the build folder 
- The BuildFolder (which contains the Builded project) needs to have the same name as your build.exe. In this Case the Buildfolder is Called: **"FahrradSim"** and the contained .exe is called **"FahrradSim.exe"**. 



#### dont use void FixedUpdate() 
- The method **FixedUpdate()** is not called by the cave, therefore always use **Update()** even for physics calculations



#### Add a NetworkIdentity component to Objects if you want to manipulate the physics
- The NetworkIdentityComponent must be part of the Bycicle component, otherwise the changes of the position will not be synced between the master and the slave PCs. 
*add picture of network Identity component*


#### Sync the position of the cave camera with the bycicle position 
- In order to sync the cave camera with the position of the bycicle it is necessary to connect the transform of the instantiated cave camera with the bycicle. This is done with the Cave Prefab **"NodeManager"** (which is always part of a cave Unity scene). 
- Within the NodeManager find the component **"Instantiate Node"** and add the "Cycle Transform" in the public editor field **"Origin Parent"**
*add Image of Instantia Node Component*



#### Run the bycicle physics calculation script only on the Master-PC
- the CaveUnity Framework is designed to share physics calculations between the master and it's slave automated. 
Therefore the physics calculations scripts must only be executed on the Master-PC. 
- The code line to find the Master-PC is: </br> **if (GetComponent**\<**NetworkIdentity**\>**().isServer)) {**</br>
  &nbsp; &nbsp; &nbsp; &nbsp;  *place physics calculations here* </br>
  }  


#### Changing the Cave camera position in relation to the Bycicle
- if the (realworld) bycicle is positioned different within the cave or the relation between the bycicle prefab components is changed, it is necessary to reposition the Cave camera. 
- In order to get a virtual represantion of the real bycicle, activate the public editor tick box **showBycicle**  
- A simple Cube and Sphre Represantation of the Bycicle will be rendered in the CavePlaymode. 
- This representation is close toward the real measurements of the Bycicle. 
- You can check if virtual and real world Models align. 
- if not, change the (x,y,z) position of the cave camera in the **"node-config.xml"** </br> *(Unityproject > Assets > Streaming Assets > node.conig.xml)*
- *add Image node-config*


## FahrradSim Unity-Project Guide


#### Receiving data from the Bycicle
- Plug in the Poweradapter of the Cycle 
- Connect the LAN cable with the PC
- Flick the ON / Off switch at the Pollin A/D changer.
- Debugging: Use the Pollin **"NetServer.exe"**, to check if the Cycle is sending Data. </br>
*"C:\Users\icuser\Desktop\CAVE_UNITY\Unity_Projects\Sebastian_K\FahrradSim\avr-net-io\Pollin-Server\NetServer.exe"*

</br>

- All Scripts concerned with the data transmission are in the Folder: *Assets > Scripts > TCP* 
- The GameObject TCP-Datastream is placed in the Scene and holds the Script: **TCP-Stream**


#### Applying the data to the InGame bycicle 
- Script: **BycicleBehaviour**
- The Angle and Speed are collected from the TCP-Stream-Script by: </br>
*angle = (float)Test_ReadData.AngleForMono;* </br>
*speed = (float)Test_ReadData.speedForMono;*
- The angle and speed are applied with the Unity Method: </br> 
*public void RotateAround(Vector3 point, Vector3 axis, float angle);*
