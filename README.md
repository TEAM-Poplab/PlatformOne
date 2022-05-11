# PlatformOne
**T.E.A.M.** is a research project that was created to facilitate the design of kinetic projects and components created through a computational design process. Time is the ingredient that allows dynamism. The application presents two key features: the first one is that everything modified and developed in the VE (Virtual Environment) retains its geometric characteristics, allowing the user to reach an informed 3D model at the end of the process; the second one is the ease and enjoyment with which the user manipulates complex dynamic geometries in the three-dimensional environment through a natural interface design approach that focuses on direct manipulation of architectural objects and components. The simulator is designed to be used in a 6DOF virtual environment using a commercial VR headset. It has currently been loaded with several archetypal test architectures and soon it will be available to designers who want to test their work with it. 

## Built with 
“Platform One” is built using **Unity** as Game Engine. The reason for this choice was mainly for the implementation of ready-to-use VR technologies, which helped us in the early stage of the prototyping research phase. As the development process went further on, we were able to develop our custom Components and built a custom framework on the top of the Oculus’ one, especially for what concerned the hand tracking feature, which was yet experimental at the start time of the research project. 

Another framework which helped us in the early stage, and that we took some elements from, is the open-source **Mixed Reality Toolkit** from Microsoft: the high customization of each Component allowed us to further change and adapt some scripts to our own necessities. A key role was determined by the lightweight yet customizable shaders of the MRTK: the abstract feeling of an ethereal virtual world beyond this reality was achievable thanks to the custom shaders of the framework. They also helped us to keep pretty high performances, keeping in mind the limited resources available in the first generation of the Oculus Quest. 

 

## Getting started 
### Prerequisites 
“Platform One” requires at least Unity 2020.3.9f to build and run. Android build support (both Android SDK and OpenJDK) module needs to be installed with Unity in order to build for the Oculus Quest platform (which is an Android-based device). Any other framework is already installed in the project. 

### Installation 
1. Download or clone the “PlatformOne” repository. 
2. Open UnityHub launcher. Select the Project tab on the right side and select Open>Add project from disk. 
3. Select the folder called “PlatformOne” containing the entire Unity project and open it. 
4. First opening should take a while since Unity has to build the local Library of assets cache, compiled shaders and so on. 
5. After the recompiling the project opens directly with PlatformOne scene as primary scene in the editor. 

 

## Usage 
### Add new custom geometries 
Any geometry in the Platform One environment is the representation of a Rhino/Grasshoper parametrized mesh in a form that Unity can visualize as a pseudo-animated mesh. The detailed process is described on the T.E.A.M. project website. 
In order to add your own “animated” mesh you have to export each frame of the Rhino animated mesh as a single unique .fbx and import them in Unity. After that, you must follow the steps below: 
1. Create a new Empty Prefab: you can name it whatever you want, but be sure to append the word “Sequence” at its name. 
2. Add each imported .fbx as child of the newly created prefab. 
3. Each .fbx should appear as a compound GameObject: a parent with a Transform component only and a child called “Mesh”, with the actual Mesh filter and Mesh renderer components 
4. Your prefab should be constructed this way at the moment: the prefab parent, a number of sub-children equal to the number of the animation frames for your custom mesh, each of them with a sub-child called “Mesh”. Add to each direct sub-children of the main prefab an Animator component and a Bolt Flow Machine component: as Controller property in the Animator choose what is called “DockableCone”, and as Macro in the Flow Machine choose what is called “MeshSequenceAnimationListener”. 
5. For any help on what a final prefab should look, open the Prefab folder in the Assets one, and in the Sequences folder inside it you can explore our built example prefabs. Have a look here to replicate the precise structure for your own prefab. 
6. In order to your custom prefab to appear at runtime, select the ImportModule GameObject in the Hierarchy panel. In the Geometry Mesh Sequence Set Module component, add a new entry in the Mesh Sequence Geometry Prefabs list and select your custom Prefab. 

### Customize the environment 
Many aspects of the experience can be customized because many features have been developed as highly changeable:
- Some parameters can be changed directly in Unity, since some of them have been exposed as public in their related script and can be found under their script component 
- Other parameters can be changed only in code, since they build up a more complex structure related to many different behaviors [each relevant script has its own documentation] 

### Build your package 
Any running build packaged from Unity has to be exported as an Android Package (.apk). The project should be already properly set. Otherwise, check into Build Settings that:
1. Android is selected as target platform. If not, select Android. 
2. Under Android settings on the right side of the panel, the Texture Compression must be set to ASTC value. 
3. If any setting has been changed, confirm changes selecting Switch Platform. A process of assets conversion starts. It should take some time depending on the hardware configuration of your computer. 
4. If Android platform is properly set, you should be able to build your own package: open the Build Settings panel, select Build (or Build and Run if your Oculus is connected to your computer and set in the Oculus app), select your output saving location on your computer and wait for the built process to complete. After that, you can upload your .apk in your Oculus Quest using the Command Prompt (on Windows) or Terminal (on Mac) or apps such as SideQuest or the Oculus Developer Hub. 

IMPORTANT: in order to upload your packages, Developer Mode for your Oculus Quest must be enabled!

## License
Copyright (c) 2022 Poplab srl 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 
 
The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software. 
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE. 
